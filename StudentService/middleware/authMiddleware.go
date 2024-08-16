package middleware

import (
    "github.com/golang-jwt/jwt/v5"
    "github.com/gin-gonic/gin"
    "net/http"
    "strings"
    "log"
    "os"
)

type CustomClaims struct {
    Email     string        `json:"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"`
    Roles     interface{}   `json:"http://schemas.microsoft.com/ws/2008/06/identity/claims/role"`
    Audiences []string      `json:"aud"`
    jwt.RegisteredClaims
}

func JWTMiddleware() gin.HandlerFunc {
    return func(c *gin.Context) {
        tokenString := c.GetHeader("Authorization")

        if tokenString == "" {
            c.JSON(http.StatusUnauthorized, gin.H{"error": "Missing Authorization Header"})
            c.Abort()
            return
        }

        tokenString = tokenString[len("Bearer "):]

        token, err := jwt.ParseWithClaims(tokenString, &CustomClaims{}, func(token *jwt.Token) (interface{}, error) {
            if _, ok := token.Method.(*jwt.SigningMethodHMAC); !ok {
                return nil, http.ErrNotSupported
            }
            return []byte(os.Getenv("JWT_KEY")), nil
        })

        if err != nil || !token.Valid {
            c.JSON(http.StatusUnauthorized, gin.H{"error": "Invalid or expired token"})
            c.Abort()
            return
        }

        claims, ok := token.Claims.(*CustomClaims)
        if !ok {
            c.JSON(http.StatusUnauthorized, gin.H{"error": "Invalid claims"})
            c.Abort()
            return
        }

        var roles []string
        switch v := claims.Roles.(type) {
        case string:
            roles = strings.Split(v, ",")
        case []interface{}:
            for _, role := range v {
                if str, ok := role.(string); ok {
                    roles = append(roles, str)
                }
            }
        }

        log.Printf("Claims: %v", claims)

        if claims.Issuer != os.Getenv("JWT_ISSUER") {
            c.JSON(http.StatusUnauthorized, gin.H{"error": "Invalid issuer"})
            c.Abort()
            return
        }

        validAudience := false
        audiences := strings.Split(os.Getenv("JWT_AUDIENCES"), ",")
        audienceList := claims.Audiences

        for _, aud := range audiences {
            trimmedAud := strings.TrimSpace(aud)
            for _, tokenAud := range audienceList {
                if tokenAud == trimmedAud {
                    validAudience = true
                    break
                }
            }
            if validAudience {
                break
            }
        }

        if !validAudience {
            c.JSON(http.StatusUnauthorized, gin.H{"error": "Invalid audience"})
            c.Abort()
            return
        }

        c.Set("email", claims.Email)
        c.Set("roles", roles)
        c.Set("audiences", claims.Audiences)

        c.Next()
    }
}