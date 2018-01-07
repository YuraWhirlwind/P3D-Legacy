﻿Public Class SignModel

    Inherits BaseModel

    Public Sub New()
        Me.ID = 4

        Dim vertexData = New VertexPositionNormalTexture() {
            New VertexPositionNormalTexture(New Vector3(-0.5, -0.5, 0.1), New Vector3(0, 0, 1), New Vector2(0.0, 1.0)), 'h
            New VertexPositionNormalTexture(New Vector3(-0.5, 0.5, 0.1), New Vector3(0, 0, 1), New Vector2(0.0, 0.0)), 'e
            New VertexPositionNormalTexture(New Vector3(0.5, -0.5, 0.1), New Vector3(0, 0, 1), New Vector2(1.0, 1.0)), 'c
            New VertexPositionNormalTexture(New Vector3(0.5, -0.5, 0.1), New Vector3(0, 0, 1), New Vector2(1.0, 1.0)), 'c
            New VertexPositionNormalTexture(New Vector3(-0.5, 0.5, 0.1), New Vector3(0, 0, 1), New Vector2(0.0, 0.0)), 'e
            New VertexPositionNormalTexture(New Vector3(0.5, 0.5, 0.1), New Vector3(0, 0, 1), New Vector2(1.0, 0.0)), 'd
            New VertexPositionNormalTexture(New Vector3(0.5, -0.5, 0.1), New Vector3(1, 0, 0), New Vector2(0.0, 1.0)), 'c
            New VertexPositionNormalTexture(New Vector3(0.5, 0.5, 0.1), New Vector3(1, 0, 0), New Vector2(0.0, 0.0)), 'd
            New VertexPositionNormalTexture(New Vector3(0.5, -0.5, -0.1), New Vector3(1, 0, 0), New Vector2(1.0, 1.0)), 'b
            New VertexPositionNormalTexture(New Vector3(0.5, -0.5, -0.1), New Vector3(1, 0, 0), New Vector2(1.0, 1.0)), 'b
            New VertexPositionNormalTexture(New Vector3(0.5, 0.5, 0.1), New Vector3(1, 0, 0), New Vector2(0.0, 0.0)), 'd
            New VertexPositionNormalTexture(New Vector3(0.5, 0.5, -0.1), New Vector3(1, 0, 0), New Vector2(1.0, 0.0)), 'g
            New VertexPositionNormalTexture(New Vector3(-0.5, 0.5, 0.1), New Vector3(-1, 0, 0), New Vector2(1.0, 0.0)), 'e
            New VertexPositionNormalTexture(New Vector3(-0.5, -0.5, 0.1), New Vector3(-1, 0, 0), New Vector2(1.0, 1.0)), 'h
            New VertexPositionNormalTexture(New Vector3(-0.5, -0.5, -0.1), New Vector3(-1, 0, 0), New Vector2(0.0, 1.0)), 'a
            New VertexPositionNormalTexture(New Vector3(-0.5, -0.5, -0.1), New Vector3(-1, 0, 0), New Vector2(0.0, 1.0)), 'a
            New VertexPositionNormalTexture(New Vector3(-0.5, 0.5, -0.1), New Vector3(-1, 0, 0), New Vector2(0.0, 0.0)), 'f
            New VertexPositionNormalTexture(New Vector3(-0.5, 0.5, 0.1), New Vector3(-1, 0, 0), New Vector2(1.0, 0.0)), 'e
            New VertexPositionNormalTexture(New Vector3(-0.5, 0.5, -0.1), New Vector3(0, 0, -1), New Vector2(1.0, 0.0)), 'f
            New VertexPositionNormalTexture(New Vector3(-0.5, -0.5, -0.1), New Vector3(0, 0, -1), New Vector2(1.0, 1.0)), 'a
            New VertexPositionNormalTexture(New Vector3(0.5, -0.5, -0.1), New Vector3(0, 0, -1), New Vector2(0.0, 1.0)), 'b
            New VertexPositionNormalTexture(New Vector3(0.5, -0.5, -0.1), New Vector3(0, 0, -1), New Vector2(0.0, 1.0)), 'b
            New VertexPositionNormalTexture(New Vector3(0.5, 0.5, -0.1), New Vector3(0, 0, -1), New Vector2(0.0, 0.0)), 'g
            New VertexPositionNormalTexture(New Vector3(-0.5, 0.5, -0.1), New Vector3(0, 0, -1), New Vector2(1.0, 0.0)) 'f
        }

        Setup(vertexData)
    End Sub

End Class
