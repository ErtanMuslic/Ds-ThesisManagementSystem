package main

import (
	pb "StudentService/proto/proto"
	"StudentService/routes"
	"context"
	"log"
	"net/http"
	"os"

	"github.com/gin-gonic/gin"
	"github.com/joho/godotenv"
	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"
)

func main() {

	err := godotenv.Load()
	if err != nil {
		log.Fatal("Error loading .env file")
	} else {
		log.Println("Successfully loaded .env file")
	}

	router := routers.EventsRouter()
	gin.SetMode(gin.ReleaseMode)


	port := os.Getenv("PORT")
	go func() {
		log.Printf("Starting HTTP server on port %s", port)
		if err := http.ListenAndServe(port, router); err != nil {
			log.Fatalf("Could not start HTTP server: %v", err)
		}
	}()

	//DEV
	grpcAddr := os.Getenv("GRPC_SERVER_ADDRESS")

	//PROD
	//grpcAddr := os.Getenv("GRPC_SERVER_ADDRESS_PROD")

	conn, err := grpc.Dial(grpcAddr, grpc.WithTransportCredentials(insecure.NewCredentials()))
	if err != nil {
		log.Printf("Failed to connect to gRPC server: %v. Continuing with other operations...", err)
	} else {
		defer conn.Close()

		client := pb.NewParticipateServiceClient(conn)

		thesis := &pb.Thesis{ThesisId: 4, Title: "Sample Thesis",StudentId: "1"}
		result, err := client.Participate(context.Background(), thesis)
		if err != nil {
			log.Printf("Error calling Participate: %v", err)
		} else {
			log.Printf("Result: %v", result)
		}
	}

	select {}
}