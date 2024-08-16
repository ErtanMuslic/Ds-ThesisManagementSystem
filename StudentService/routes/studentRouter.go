package routers

import (
	"StudentService/controller"
	"StudentService/middleware"
	"github.com/gin-gonic/gin"
)

func EventsRouter() *gin.Engine {
    router := gin.Default()

    router.Use(middleware.JWTMiddleware())

    eventsGroup := router.Group(StudentBaseRoute)
    {
        eventsGroup.POST("/Add", middleware.RequireRoles("Professor"),studentController.AddStudent)
        eventsGroup.GET("/Get/:id", middleware.RequireRoles("Professor"), studentController.GetStudentByID)
        eventsGroup.GET("/GetAll", middleware.RequireRoles("Professor"), studentController.GetAllStudents)
        eventsGroup.DELETE("/Delete/:id",middleware.RequireRoles("Admin"), studentController.DeleteStudent)
        eventsGroup.PATCH("/Update/:id", middleware.RequireRoles("Admin"), studentController.UpdateStudent)
        eventsGroup.POST("/:studentId/apply/:thesisId", middleware.RequireRoles("Professor"), studentController.ApplyForThesis)
    }

    return router
}