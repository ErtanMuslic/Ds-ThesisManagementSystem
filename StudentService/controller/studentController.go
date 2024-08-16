package studentController

import (
	"StudentService/models"
	pb "StudentService/proto/proto"
	"context"
	"fmt"
	"log"
	"net/http"
	"os"
	"strconv"
	"time"

	"github.com/gin-gonic/gin"
	"github.com/joho/godotenv"
	"go.mongodb.org/mongo-driver/bson"
	"go.mongodb.org/mongo-driver/bson/primitive"
	"go.mongodb.org/mongo-driver/mongo"
	"go.mongodb.org/mongo-driver/mongo/options"
	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"
)

var studentsCollection *mongo.Collection

func init() {
	err := godotenv.Load(".env")
	if err != nil {
		log.Fatalf("Error loading .env file")
	}

	connectionString := os.Getenv("MONGODB_CONNECTION_STRING")
	fmt.Println("Connection string: " + connectionString)

	clientOptions := options.Client().ApplyURI(connectionString)
	client, err := mongo.Connect(context.TODO(), clientOptions)
	if err != nil {
		log.Fatal(err)
	}

	err = client.Ping(context.TODO(), nil)
	if err != nil {
		log.Fatal(err)
	}

	fmt.Println("Mongodb connection success")

	dbName := os.Getenv("DB_NAME")
	colName := "students"

	studentsCollection = client.Database(dbName).Collection(colName)

	fmt.Println("Collection instance is ready")
}

func AddStudent(c *gin.Context) {
	var ctx, cancel = context.WithCancel(context.Background())
	defer cancel()

	var newStudent models.Student
	if err := c.BindJSON(&newStudent); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	newStudent.ID = primitive.NewObjectID()
	newStudent.CreatedAt = primitive.NewDateTimeFromTime(time.Now())

	_, err := studentsCollection.InsertOne(ctx, newStudent)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to add student"})
		return
	}

	c.JSON(http.StatusCreated, newStudent)
}

func GetStudentByID(c *gin.Context) {
	var ctx, cancel = context.WithCancel(context.Background())
	defer cancel()

	id := c.Param("id")
	objID, err := primitive.ObjectIDFromHex(id)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid ID"})
		return
	}

	var student models.Student
	err = studentsCollection.FindOne(ctx, bson.M{"_id": objID}).Decode(&student)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to fetch student"})
		return
	}

	c.JSON(http.StatusOK, student)
}

func UpdateStudent(c *gin.Context) {
	var ctx, cancel = context.WithCancel(context.Background())
	defer cancel()

	id := c.Param("id")
	objID, err := primitive.ObjectIDFromHex(id)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid ID"})
		return
	}

	var updatedStudent models.Student
	if err := c.BindJSON(&updatedStudent); err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": err.Error()})
		return
	}

	update := bson.M{
		"$set": bson.M{
			"name":         updatedStudent.Name,
			"age":          updatedStudent.Age,
			"academicYear": updatedStudent.AcademicYear,
			"thesisID":     updatedStudent.ThesisID,
		},
	}

	_, err = studentsCollection.UpdateOne(ctx, bson.M{"_id": objID}, update)
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to update student"})
		return
	}

	c.JSON(http.StatusOK, gin.H{"message": "Student updated successfully"})
}

func DeleteStudent(c *gin.Context) {
	var ctx, cancel = context.WithCancel(context.Background())
	defer cancel()

	id := c.Param("id")
	objID, err := primitive.ObjectIDFromHex(id)
	if err != nil {
		c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid ID"})
		return
	}

	_, err = studentsCollection.DeleteOne(ctx, bson.M{"_id": objID})
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to delete student"})
		return
	}

	c.JSON(http.StatusOK, gin.H{"message": "Student with ID " + id + " has been deleted"})
}

func GetAllStudents(c *gin.Context) {
	var ctx, cancel = context.WithCancel(context.Background())
	defer cancel()

	cursor, err := studentsCollection.Find(ctx, bson.M{})
	if err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to fetch students"})
		return
	}
	defer cursor.Close(ctx)

	var students []models.Student
	if err = cursor.All(ctx, &students); err != nil {
		c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to parse students"})
		return
	}

	c.JSON(http.StatusOK, students)
}

func ApplyForThesis(c *gin.Context) {
	studentId := c.Param("studentId")
	thesisId := c.Param("thesisId")
	token := c.GetHeader("Authorization")
	thesisIdInt, err := strconv.Atoi(thesisId)
	if err != nil {
		log.Printf("Error parsing thesisId: %v", err)
		c.JSON(http.StatusInternalServerError, gin.H{"error": "invalid thesisId"})
		return
	}

	// Connect to gRPC server
	//DEV:
	//grpcAddr := os.Getenv("GRPC_SERVER_ADDRESS")

	//PROD:
	grpcAddr := os.Getenv("GRPC_SERVER_ADDRESS_PROD")
	conn, err := grpc.Dial(grpcAddr, grpc.WithTransportCredentials(insecure.NewCredentials()))
	if err != nil {
		log.Printf("Failed to connect to gRPC server: %v", err)
		c.JSON(http.StatusInternalServerError, gin.H{"error": "failed to connect to gRPC server"})
		return
	}
	defer conn.Close()

	client := pb.NewParticipateServiceClient(conn)

	thesis := &pb.Thesis{
		ThesisId:  int32(thesisIdInt),
		StudentId: studentId,
		Token:     token,
	}

	response, err := client.Participate(c, thesis)
	if err != nil {
		log.Printf("Error calling Participate: %v", err)
		c.JSON(http.StatusInternalServerError, gin.H{"error": "gRPC call failed"})
		return
	}

	if response.Approved {
		var ctx, cancel = context.WithCancel(context.Background())
		defer cancel()

		objID, err := primitive.ObjectIDFromHex(studentId)
		if err != nil {
			c.JSON(http.StatusBadRequest, gin.H{"error": "Invalid student ID"})
			return
		}

		var student models.Student
		err = studentsCollection.FindOne(ctx, bson.M{"_id": objID}).Decode(&student)
		if err != nil {
			c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to fetch student"})
			return
		}

		update := bson.M{
			"$set": bson.M{
				"thesisID": thesisIdInt,
			},
		}

		_, err = studentsCollection.UpdateOne(ctx, bson.M{"_id": objID}, update)
		if err != nil {
			c.JSON(http.StatusInternalServerError, gin.H{"error": "Failed to update student's thesisId"})
			return
		}

		log.Printf("Student with ID %s successfully applied for thesis with ID %d", studentId, thesisIdInt)
		c.JSON(http.StatusOK, gin.H{"message": "Student's thesisId updated successfully", "approved": response.Approved})
	} else {
		c.JSON(http.StatusOK, gin.H{"message": "Thesis application not approved", "approved": response.Approved})
	}


}