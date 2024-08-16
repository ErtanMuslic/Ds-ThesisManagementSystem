package models

import (
    "go.mongodb.org/mongo-driver/bson/primitive"
)

type Student struct {
    ID          	primitive.ObjectID   `bson:"_id,omitempty" json:"id"`
    Name        	string               `bson:"name" json:"name"`
    Age 			int               	 `bson:"age" json:"age"`
    AcademicYear	int               	 `bson:"academicYear" json:"academicYear"`
    CreatedAt   	primitive.DateTime   `bson:"createdAt" json:"createdAt"`
    ThesisID  		int                  `bson:"thesisID" json:"thesisID"` 
}