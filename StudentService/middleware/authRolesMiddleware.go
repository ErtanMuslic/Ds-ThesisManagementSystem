package middleware

import (
    "github.com/gin-gonic/gin"
    "net/http"
    "log"
)

func RequireRoles(roles ...string) gin.HandlerFunc {
    return func(c *gin.Context) {
        userRoles, exists := c.Get("roles")

        log.Printf("userRoles: %v", userRoles)

        if !exists {
            c.JSON(http.StatusForbidden, gin.H{"error": "Roles not found"})
            c.Abort()
            return
        }

        userRolesSlice, ok := userRoles.([]string)
        if !ok {
            c.JSON(http.StatusForbidden, gin.H{"error": "Invalid roles format"})
            c.Abort()
            return
        }

        roleFound := false
        for _, role := range roles {
            for _, userRole := range userRolesSlice {
                if role == userRole {
                    roleFound = true
                    break
                }
            }
            if roleFound {
                break
            }
        }

        if !roleFound {
            c.JSON(http.StatusForbidden, gin.H{"error": "Access denied"})
            c.Abort()
            return
        }

        c.Next()
    }
}