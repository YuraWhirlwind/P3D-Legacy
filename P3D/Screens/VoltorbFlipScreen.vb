﻿Imports P3D.Screens.UI

Namespace VoltorbFlip
    Public Class VoltorbFlipScreen

        Inherits Screen

        ' Variables & Properties

        Private _screenTransitionY As Single = 0F
        Public Shared _interfaceFade As Single = 0F

        Private Delay As Integer = 0
        Private MemoMenuX As Single = 0F
        Private MemoMenuSize As New Size(112, 112)

        Private Shared ReadOnly GameSize As New Size(576, 544)
        Public Shared ReadOnly BoardSize As New Size(384, 384)
        Public Shared ReadOnly TileSize As New Size(64, 64)
        Private Shared ReadOnly GridSize As Integer = 5

        Public Shared GameOrigin As New Vector2(CInt(windowSize.Width / 2 - GameSize.Width / 2), CInt(windowSize.Height / 2 - GameSize.Height / 2))
        Public Shared BoardOrigin As New Vector2(GameOrigin.X + 32, GameOrigin.Y + 160)

        Private BoardCursorPosition As New Vector2(0, 0)
        Private BoardCursorDestination As New Vector2(0, 0)

        Private MemoIndex As Integer = 0

        Public Shared GameState As States = States.Opening

        Public Shared Property PreviousLevel As Integer = 1
        Public Shared Property CurrentLevel As Integer = 1

        Public Shared ReadOnly MinLevel As Integer = 1
        Public Shared ReadOnly MaxLevel As Integer = 7
        Public Shared Property CurrentFlips As Integer = 0
        Public Shared Property TotalFlips As Integer = 0

        Public Shared Property CurrentCoins As Integer = 0
        Public Shared Property TotalCoins As Integer = -1
        Public Shared Property ConsequentWins As Integer = 0
        Public Shared MaxCoins As Integer = 1

        Public Board As List(Of List(Of Tile))

        Public VoltorbSums As List(Of List(Of Integer))
        Public CoinSums As List(Of List(Of Integer))

        Public Enum States
            Opening
            Closing
            QuitQuestion
            Game
            Memo
            GameWon
            GameLost
            FlipWon
            FlipLost
            NewLevel
        End Enum

        'Stuff related to blurred PreScreens
        Private _blur As Resources.Blur.BlurHandler
        Private _preScreenTexture As RenderTarget2D
        Private _preScreenTarget As RenderTarget2D
        Private _blurScreens As Identifications() = {Identifications.BattleScreen,
                                                 Identifications.OverworldScreen,
                                                 Identifications.DirectTradeScreen,
                                                 Identifications.WonderTradeScreen,
                                                 Identifications.GTSSetupScreen,
                                                 Identifications.GTSTradeScreen,
                                                 Identifications.PVPLobbyScreen}

        Public Sub New(ByVal currentScreen As Screen)
            GameState = States.Opening
            GameOrigin = New Vector2(CInt(windowSize.Width / 2 - GameSize.Width / 2), CInt(windowSize.Height / 2 - _screenTransitionY))
            BoardOrigin = New Vector2(GameOrigin.X + 32, GameOrigin.Y + 160)
            BoardCursorDestination = GetCursorOffset(0, 0)
            BoardCursorPosition = GetCursorOffset(0, 0)

            Board = CreateBoard(CurrentLevel)
            TotalCoins = 0

            _preScreenTarget = New RenderTarget2D(GraphicsDevice, windowSize.Width, windowSize.Height, False, SurfaceFormat.Color, DepthFormat.Depth24Stencil8)
            _blur = New Resources.Blur.BlurHandler(windowSize.Width, windowSize.Height)

            Identification = Identifications.VoltorbFlipScreen
            PreScreen = currentScreen
            IsDrawingGradients = True

            Me.MouseVisible = True
            Me.CanChat = Me.PreScreen.CanChat
            Me.CanBePaused = Me.PreScreen.CanBePaused

        End Sub


        Public Overrides Sub Draw()
            If _blurScreens.Contains(PreScreen.Identification) Then
                DrawPrescreen()
            Else
                PreScreen.Draw()
            End If


            DrawGradients(CInt(255 * _interfaceFade))

            ChooseBox.Draw()
            TextBox.Draw()

            DrawBackground()

            DrawMemoMenuAndButton()

            If Board IsNot Nothing Then
                DrawBoard()
                DrawCursor()
            End If
            DrawHUD()
            DrawQuitButton()


        End Sub

        Private Sub DrawPrescreen()
            If _preScreenTexture Is Nothing OrElse _preScreenTexture.IsContentLost Then
                SpriteBatch.EndBatch()

                Dim target As RenderTarget2D = _preScreenTarget
                GraphicsDevice.SetRenderTarget(target)
                GraphicsDevice.Clear(BackgroundColor)

                SpriteBatch.BeginBatch()

                PreScreen.Draw()

                SpriteBatch.EndBatch()

                GraphicsDevice.SetRenderTarget(Nothing)

                SpriteBatch.BeginBatch()

                _preScreenTexture = target
            End If

            If _interfaceFade < 1.0F Then
                SpriteBatch.Draw(_preScreenTexture, windowSize, Color.White)
            End If
            SpriteBatch.Draw(_blur.Perform(_preScreenTexture), windowSize, New Color(255, 255, 255, CInt(255 * _interfaceFade * 2).Clamp(0, 255)))
        End Sub

        Private Sub DrawBackground()
            Dim mainBackgroundColor As Color = New Color(255, 255, 255)
            If GameState = States.Closing Or GameState = States.Opening Then
                mainBackgroundColor = New Color(255, 255, 255, CInt(255 * _interfaceFade))
            End If

            Canvas.DrawImageBorder(TextureManager.GetTexture("Textures\VoltorbFlip\Background"), 2, New Rectangle(CInt(GameOrigin.X), CInt(GameOrigin.Y), CInt(GameSize.Width), CInt(GameSize.Height)), mainBackgroundColor, False)

        End Sub
        Private Sub DrawHUD()
            Dim mainBackgroundColor As Color = New Color(255, 255, 255)
            If GameState = States.Closing Or GameState = States.Opening Then
                mainBackgroundColor = New Color(255, 255, 255, CInt(255 * _interfaceFade))
            End If

            Dim Fontcolor As Color = New Color(0, 0, 0)
            If GameState = States.Closing Or GameState = States.Opening Then
                Fontcolor = New Color(0, 0, 0, CInt(255 * _interfaceFade))
            End If

            'Level
            Dim LevelText As String = "LV." & CurrentLevel.ToString
            Canvas.DrawImageBorder(TextureManager.GetTexture("Textures\VoltorbFlip\HUD"), 2, New Rectangle(CInt(GameOrigin.X + 32), CInt(GameOrigin.Y + 32), 96, 96), mainBackgroundColor, False)
            SpriteBatch.DrawString(FontManager.MainFont, LevelText, New Vector2(CInt(GameOrigin.X + 80 + 4 - FontManager.MainFont.MeasureString(LevelText).X / 2), CInt(GameOrigin.Y + 80 + 4 - FontManager.MainFont.MeasureString(LevelText).Y / 2)), Fontcolor)

            'Current Coins
            Canvas.DrawImageBorder(TextureManager.GetTexture("Textures\VoltorbFlip\HUD"), 2, New Rectangle(CInt(GameOrigin.X + 128 + 24), CInt(GameOrigin.Y + 32), 192, 96), mainBackgroundColor, False)

            Dim CurrentCoinsText1 As String = "Coins found"
            Dim CurrentCoinsText2 As String = "in this LV."
            Dim CurrentCoinsText3 As String = ""

            CurrentCoinsText3 &= "["
            If CurrentCoins < 10000 Then
                CurrentCoinsText3 &= "0"
            End If
            If CurrentCoins < 1000 Then
                CurrentCoinsText3 &= "0"
            End If
            If CurrentCoins < 100 Then
                CurrentCoinsText3 &= "0"
            End If
            If CurrentCoins < 10 Then
                CurrentCoinsText3 &= "0"
            End If
            CurrentCoinsText3 &= CurrentCoins.ToString & "]"

            SpriteBatch.DrawString(FontManager.MainFont, CurrentCoinsText1, New Vector2(CInt(GameOrigin.X + 232 + 20 - FontManager.MainFont.MeasureString(CurrentCoinsText1).X / 2), CInt(GameOrigin.Y + 80 + 4 - FontManager.MainFont.MeasureString(CurrentCoinsText2).Y / 2 - FontManager.MainFont.MeasureString(CurrentCoinsText1).Y)), Fontcolor)
            SpriteBatch.DrawString(FontManager.MainFont, CurrentCoinsText2, New Vector2(CInt(GameOrigin.X + 232 + 20 - FontManager.MainFont.MeasureString(CurrentCoinsText2).X / 2), CInt(GameOrigin.Y + 80 + 4 - FontManager.MainFont.MeasureString(CurrentCoinsText2).Y / 2)), Fontcolor)
            SpriteBatch.DrawString(FontManager.MainFont, CurrentCoinsText3, New Vector2(CInt(GameOrigin.X + 232 + 20 - FontManager.MainFont.MeasureString(CurrentCoinsText3).X / 2), CInt(GameOrigin.Y + 80 + 4 + FontManager.MainFont.MeasureString(CurrentCoinsText2).Y / 2)), Fontcolor)

            'Total Coins
            Canvas.DrawImageBorder(TextureManager.GetTexture("Textures\VoltorbFlip\HUD"), 2, New Rectangle(CInt(GameOrigin.X + 336 + 32), CInt(GameOrigin.Y + 32), 192, 96), mainBackgroundColor, False)

            Dim TotalCoinsText1 As String = "Total Coins"
            Dim TotalCoinsText2 As String = "earned"
            Dim TotalCoinsText3 As String = ""

            TotalCoinsText3 &= "["
            If TotalCoins < 10000 Then
                TotalCoinsText3 &= "0"
            End If
            If TotalCoins < 1000 Then
                TotalCoinsText3 &= "0"
            End If
            If TotalCoins < 100 Then
                TotalCoinsText3 &= "0"
            End If
            If TotalCoins < 10 Then
                TotalCoinsText3 &= "0"
            End If
            TotalCoinsText3 &= TotalCoins.ToString & "]"

            SpriteBatch.DrawString(FontManager.MainFont, TotalCoinsText1, New Vector2(CInt(GameOrigin.X + 440 + 28 - FontManager.MainFont.MeasureString(TotalCoinsText1).X / 2), CInt(GameOrigin.Y + 80 + 4 - FontManager.MainFont.MeasureString(TotalCoinsText2).Y / 2 - FontManager.MainFont.MeasureString(TotalCoinsText1).Y)), Fontcolor)
            SpriteBatch.DrawString(FontManager.MainFont, TotalCoinsText2, New Vector2(CInt(GameOrigin.X + 440 + 28 - FontManager.MainFont.MeasureString(TotalCoinsText2).X / 2), CInt(GameOrigin.Y + 80 + 4 - FontManager.MainFont.MeasureString(TotalCoinsText2).Y / 2)), Fontcolor)
            SpriteBatch.DrawString(FontManager.MainFont, TotalCoinsText3, New Vector2(CInt(GameOrigin.X + 440 + 28 - FontManager.MainFont.MeasureString(TotalCoinsText3).X / 2), CInt(GameOrigin.Y + 80 + 4 + FontManager.MainFont.MeasureString(TotalCoinsText2).Y / 2)), Fontcolor)


        End Sub

        Private Sub DrawBoard()
            Dim mainBackgroundColor As Color = New Color(255, 255, 255)
            If GameState = States.Closing Or GameState = States.Opening Then
                mainBackgroundColor = New Color(255, 255, 255, CInt(255 * _interfaceFade))
            End If

            SpriteBatch.Draw(TextureManager.GetTexture("Textures\VoltorbFlip\Board"), New Rectangle(CInt(BoardOrigin.X), CInt(BoardOrigin.Y), BoardSize.Width, BoardSize.Height), mainBackgroundColor)

            DrawTiles()

            DrawSums()

        End Sub

        Private Sub DrawTiles()
            For _row = 0 To GridSize - 1
                For _column = 0 To GridSize - 1
                    Dim _tile As Tile = Board(_row)(_column)
                    _tile.Draw()
                Next
            Next
        End Sub

        Private Sub DrawSums()
            Dim mainBackgroundColor As Color = New Color(255, 255, 255)
            If GameState = States.Closing Or GameState = States.Opening Then
                mainBackgroundColor = New Color(255, 255, 255, CInt(255 * _interfaceFade))
            End If

            'Draw Rows
            'Coins
            For RowIndex = 0 To GridSize - 1
                Dim CoinSumString As String = "00"
                If GameState = States.Game Or GameState = States.Memo Or GameState = States.QuitQuestion Then
                    Dim CoinSumInteger As Integer = CoinSums(0)(RowIndex)
                    If CoinSumInteger < 10 Then
                        CoinSumString = "0" & CoinSumInteger.ToString
                    Else
                        CoinSumString = CoinSumInteger.ToString
                    End If
                End If
                SpriteBatch.DrawString(FontManager.VoltorbFlipFont, CoinSumString, New Vector2(CInt(BoardOrigin.X + TileSize.Width * (GridSize + 1) - 8 - FontManager.VoltorbFlipFont.MeasureString(CoinSumString).X), BoardOrigin.Y + TileSize.Height * RowIndex + 8), mainBackgroundColor)
            Next
            'Voltorbs
            For RowIndex = 0 To GridSize - 1
                Dim VoltorbSumString As String = "0"
                If GameState = States.Game Or GameState = States.Memo Or GameState = States.QuitQuestion Then
                    VoltorbSumString = VoltorbSums(0)(RowIndex).ToString
                End If
                SpriteBatch.DrawString(FontManager.VoltorbFlipFont, VoltorbSumString, New Vector2(CInt(BoardOrigin.X + TileSize.Width * (GridSize + 1) - 8 - FontManager.VoltorbFlipFont.MeasureString(VoltorbSumString).X), BoardOrigin.Y + TileSize.Height * RowIndex + 34), mainBackgroundColor)
            Next

            'Draw Columns
            'Coins
            For ColumnIndex = 0 To GridSize - 1
                Dim CoinSumString As String = "00"
                If GameState = States.Game Or GameState = States.Memo Then
                    Dim CoinSumInteger As Integer = CoinSums(1)(ColumnIndex)
                    If CoinSumInteger < 10 Then
                        CoinSumString = "0" & CoinSumInteger.ToString
                    Else
                        CoinSumString = CoinSumInteger.ToString
                    End If
                End If
                SpriteBatch.DrawString(FontManager.VoltorbFlipFont, CoinSumString, New Vector2(CInt(BoardOrigin.X + TileSize.Width * ColumnIndex + TileSize.Width - 8 - FontManager.VoltorbFlipFont.MeasureString(CoinSumString).X), BoardOrigin.Y + TileSize.Height * GridSize + 8), mainBackgroundColor)
            Next
            'Voltorbs
            For ColumnIndex = 0 To GridSize - 1
                Dim VoltorbSumString As String = "0"
                If GameState = States.Game Or GameState = States.Memo Then
                    VoltorbSumString = VoltorbSums(1)(ColumnIndex).ToString
                End If
                SpriteBatch.DrawString(FontManager.VoltorbFlipFont, VoltorbSumString, New Vector2(CInt(BoardOrigin.X + TileSize.Width * ColumnIndex + TileSize.Width - 8 - FontManager.VoltorbFlipFont.MeasureString(VoltorbSumString).X), BoardOrigin.Y + TileSize.Height * GridSize + 34), mainBackgroundColor)
            Next

        End Sub

        Private Sub DrawMemoMenuAndButton()
            Dim mainBackgroundColor As Color = New Color(255, 255, 255)
            If GameState = States.Closing Or GameState = States.Opening Then
                mainBackgroundColor = New Color(255, 255, 255, CInt(255 * _interfaceFade))
            End If
            Dim Fontcolor As Color = New Color(0, 0, 0)
            If GameState = States.Closing Or GameState = States.Opening Then
                Fontcolor = New Color(0, 0, 0, CInt(255 * _interfaceFade))
            End If
            'Draw Button
            Dim ButtonOriginX As Integer = CInt(BoardOrigin.X + BoardSize.Width + TileSize.Width / 4)
            SpriteBatch.Draw(TextureManager.GetTexture("VoltorbFlip\Memo_Button", New Rectangle(0, 0, 56, 56)), New Rectangle(ButtonOriginX, CInt(BoardOrigin.Y), MemoMenuSize.Width, MemoMenuSize.Height), mainBackgroundColor)

            Dim ButtonTextTop As String = "Open"
            Dim ButtonTextBottom As String = "Memos"

            If GameState = States.Memo Then
                ButtonTextTop = "Close"
            End If

            SpriteBatch.DrawString(FontManager.MainFont, ButtonTextTop, New Vector2(CInt(ButtonOriginX + MemoMenuSize.Width / 2 - FontManager.MainFont.MeasureString(ButtonTextTop).X / 2), CInt(BoardOrigin.Y + 40)), Fontcolor)
            SpriteBatch.DrawString(FontManager.MainFont, ButtonTextBottom, New Vector2(CInt(ButtonOriginX + MemoMenuSize.Width / 2 - FontManager.MainFont.MeasureString(ButtonTextBottom).X / 2), CInt(BoardOrigin.Y + 40 + FontManager.MainFont.MeasureString(ButtonTextTop).Y)), Fontcolor)

            'Draw Memo Menu
            If MemoMenuX > 0 Then

                Dim CurrentTile As Tile = Board(CInt(GetCurrentTile.Y))(CInt(GetCurrentTile.X))

                'Draw Background
                SpriteBatch.Draw(TextureManager.GetTexture("VoltorbFlip\Memo_Background", New Rectangle(0, 0, 56, 56)), New Rectangle(CInt(BoardOrigin.X + BoardSize.Width - MemoMenuSize.Width + MemoMenuX), CInt(BoardOrigin.Y + MemoMenuSize.Height + TileSize.Height / 2), MemoMenuSize.Width, MemoMenuSize.Height), mainBackgroundColor)

                If GameState = States.Memo Then
                    'Draw lit up Memos in the Memo menu when it's enabled on a tile
                    If CurrentTile.GetMemo(0) = True Then 'Voltorb
                        SpriteBatch.Draw(TextureManager.GetTexture("VoltorbFlip\Memo_Enabled", New Rectangle(0, 0, 56, 56)), New Rectangle(CInt(BoardOrigin.X + BoardSize.Width - MemoMenuSize.Width + MemoMenuX), CInt(BoardOrigin.Y + MemoMenuSize.Height + TileSize.Height / 2), MemoMenuSize.Width, MemoMenuSize.Height), mainBackgroundColor)
                    End If
                    If CurrentTile.GetMemo(1) = True Then 'x1
                        SpriteBatch.Draw(TextureManager.GetTexture("VoltorbFlip\Memo_Enabled", New Rectangle(56, 0, 56, 56)), New Rectangle(CInt(BoardOrigin.X + BoardSize.Width - MemoMenuSize.Width + MemoMenuX), CInt(BoardOrigin.Y + MemoMenuSize.Height + TileSize.Height / 2), MemoMenuSize.Width, MemoMenuSize.Height), mainBackgroundColor)
                    End If
                    If CurrentTile.GetMemo(2) = True Then 'x2
                        SpriteBatch.Draw(TextureManager.GetTexture("VoltorbFlip\Memo_Enabled", New Rectangle(56 + 56, 0, 56, 56)), New Rectangle(CInt(BoardOrigin.X + BoardSize.Width - MemoMenuSize.Width + MemoMenuX), CInt(BoardOrigin.Y + MemoMenuSize.Height + TileSize.Height / 2), MemoMenuSize.Width, MemoMenuSize.Height), mainBackgroundColor)
                    End If
                    If CurrentTile.GetMemo(3) = True Then 'x3
                        SpriteBatch.Draw(TextureManager.GetTexture("VoltorbFlip\Memo_Enabled", New Rectangle(56 + 56 + 56, 0, 56, 56)), New Rectangle(CInt(BoardOrigin.X + BoardSize.Width - MemoMenuSize.Width + MemoMenuX), CInt(BoardOrigin.Y + MemoMenuSize.Height + TileSize.Height / 2), MemoMenuSize.Width, MemoMenuSize.Height), mainBackgroundColor)
                    End If

                    'Draw indicator of currently selected Memo
                    SpriteBatch.Draw(TextureManager.GetTexture("VoltorbFlip\Memo_Index", New Rectangle(56 * MemoIndex, 0, 56, 56)), New Rectangle(CInt(BoardOrigin.X + BoardSize.Width - MemoMenuSize.Width + MemoMenuX), CInt(BoardOrigin.Y + MemoMenuSize.Height + TileSize.Height / 2), MemoMenuSize.Width, MemoMenuSize.Height), mainBackgroundColor)
                End If
            End If

        End Sub
        Private Sub DrawCursor()
            If GameState = States.Game OrElse GameState = States.Memo Then
                Dim mainBackgroundColor As Color = New Color(255, 255, 255)
                If GameState = States.Closing Or GameState = States.Opening Then
                    mainBackgroundColor = New Color(255, 255, 255, CInt(255 * _interfaceFade))
                End If

                Dim CursorImage As Texture2D = TextureManager.GetTexture("Textures\VoltorbFlip\Cursor_Game")
                If GameState = States.Memo Then
                    CursorImage = TextureManager.GetTexture("Textures\VoltorbFlip\Cursor_Memo")
                End If

                SpriteBatch.Draw(CursorImage, New Rectangle(CInt(VoltorbFlipScreen.BoardOrigin.X + BoardCursorPosition.X), CInt(VoltorbFlipScreen.BoardOrigin.Y + BoardCursorPosition.Y), TileSize.Width, TileSize.Height), mainBackgroundColor)
            End If
        End Sub
        Private Sub DrawQuitButton()
            Dim mainBackgroundColor As Color = New Color(255, 255, 255)
            If GameState = States.Closing Or GameState = States.Opening Then
                mainBackgroundColor = New Color(255, 255, 255, CInt(255 * _interfaceFade))
            End If
            Dim QuitButtonRectangle As New Rectangle(CInt(GameOrigin.X + 424), CInt(GameOrigin.Y + 448), 128, 56)
            SpriteBatch.Draw(TextureManager.GetTexture("Textures\VoltorbFlip\Quit_Button"), QuitButtonRectangle, mainBackgroundColor)
        End Sub

        Private Function CreateBoard(ByVal Level As Integer) As List(Of List(Of Tile))

            Dim Board As List(Of List(Of Tile)) = CreateGrid()
            Dim Data As List(Of Integer) = GetLevelData(Level)
            Dim Spots As List(Of List(Of Integer)) = New List(Of List(Of Integer))

            For i = 0 To Data(0) + Data(1) + Data(2) - 1
                If Spots.Count > 0 Then
                    Dim ValueX As Integer = Random.Next(0, 5)
                    Dim ValueY As Integer = Random.Next(0, 5)
TryAgain:
                    Dim IsUnique As Boolean = True
                    For SpotIndex = 0 To Spots.Count - 1
                        If Spots(SpotIndex)(0) = ValueX AndAlso Spots(SpotIndex)(1) = ValueY Then
                            IsUnique = False
                            Exit For
                        End If
                    Next

                    If IsUnique = False Then
                        ValueX = Random.Next(0, 5)
                        ValueY = Random.Next(0, 5)
                        GoTo TryAgain
                    Else
                        Spots.Add(New List(Of Integer)({ValueX, ValueY}.ToList))
                    End If
                Else
                    Spots.Add(New List(Of Integer)({Random.Next(0, 5), Random.Next(0, 5)}.ToList))
                End If
            Next

            If Data(0) > 0 Then
                For a = 0 To Data(0) - 1
                    Dim TileX As Integer = Spots(a)(0)
                    Dim TileY As Integer = Spots(a)(1)
                    Board(TileY)(TileX).Value = Tile.Values.Two
                Next
            End If

            If Data(1) > 0 Then
                For b = 0 To Data(1) - 1
                    Dim TileX As Integer = Spots(b + Data(0))(0)
                    Dim TileY As Integer = Spots(b + Data(0))(1)

                    Board(TileY)(TileX).Value = Tile.Values.Three
                Next
            End If

            If Data(2) > 0 Then
                For c = 0 To Data(2) - 1
                    Dim TileX As Integer = Spots(c + Data(0) + Data(1))(0)
                    Dim TileY As Integer = Spots(c + Data(0) + Data(1))(1)

                    Board(TileY)(TileX).Value = Tile.Values.Voltorb
                Next
            End If

            If Data(0) > 0 AndAlso Data(1) > 0 Then
                MaxCoins = CInt(Math.Pow(2, Data(0)) * Math.Pow(3, Data(1)))
            End If
            If Data(0) > 0 AndAlso Data(1) = 0 Then
                MaxCoins = CInt(Math.Pow(2, Data(0)))
            End If
            If Data(0) = 0 AndAlso Data(1) > 0 Then
                MaxCoins = CInt(Math.Pow(3, Data(1)))
            End If

            VoltorbSums = GenerateSums(Board, True)
            CoinSums = GenerateSums(Board, False)

            Return Board

        End Function

        ''' <summary>
        ''' Returns an empty grid of Tiles
        ''' </summary>
        ''' <returns></returns>
        Private Function CreateGrid() As List(Of List(Of Tile))
            Dim Grid As New List(Of List(Of Tile))
            For _row = 0 To VoltorbFlipScreen.GridSize - 1
                Dim Column As New List(Of Tile)
                For _column = 0 To VoltorbFlipScreen.GridSize - 1
                    Column.Add(New VoltorbFlip.Tile(_row, _column, VoltorbFlip.Tile.Values.One, False))
                Next
                Grid.Add(Column)
            Next
            Return Grid
        End Function

        ''' <summary>
        ''' Returns amount of either Coins or Voltorbs in each row and column of a grid of Tiles
        ''' </summary>
        ''' <param name="Board"></param> A grid of Tiles
        ''' <param name="CoinsOrVoltorbs"></param> True returns amount of Voltorbs, False returns amount of Coins
        ''' <returns></returns>
        Private Function GenerateSums(ByVal Board As List(Of List(Of Tile)), ByVal CoinsOrVoltorbs As Boolean) As List(Of List(Of Integer))
            Dim RowSums As New List(Of Integer)
            Dim ColumnSums As New List(Of Integer)
            Dim RowBombs As New List(Of Integer)
            Dim ColumnBombs As New List(Of Integer)

            RowSums.AddRange({0, 0, 0, 0, 0}.ToList)
            ColumnSums.AddRange({0, 0, 0, 0, 0}.ToList)

            RowBombs.AddRange({0, 0, 0, 0, 0}.ToList)
            ColumnBombs.AddRange({0, 0, 0, 0, 0}.ToList)

            'Rows
            For _row = 0 To GridSize - 1
                For _column = 0 To GridSize - 1
                    If Board(_row)(_column).Value = Tile.Values.Voltorb Then
                        RowBombs(_row) += 1
                    Else
                        RowSums(_row) += Board(_row)(_column).Value
                    End If
                Next
            Next

            'Columns
            For _column = 0 To GridSize - 1
                For _row = 0 To GridSize - 1
                    If Board(_row)(_column).Value = Tile.Values.Voltorb Then
                        ColumnBombs(_column) += 1
                    Else
                        ColumnSums(_column) += Board(_row)(_column).Value
                    End If
                Next
            Next

            If CoinsOrVoltorbs = False Then
                Dim Sums As New List(Of List(Of Integer))
                Sums.AddRange({RowSums, ColumnSums})
                Return Sums
            Else
                Dim Voltorbs As New List(Of List(Of Integer))
                Voltorbs.AddRange({RowBombs, ColumnBombs})
                Return Voltorbs
            End If

        End Function

        Public Function GetCursorOffset(Optional ByVal Column As Integer = 0, Optional ByVal Row As Integer = 0) As Vector2
            Return New Vector2(TileSize.Width * Column, TileSize.Height * Row)
        End Function

        ''' <summary>
        ''' Get the tile that the cursor is on
        ''' </summary>
        ''' <returns></returns>
        Public Function GetCurrentTile() As Vector2
            Return New Vector2((BoardCursorDestination.X / TileSize.Width).Clamp(0, GridSize - 1), (BoardCursorDestination.Y / TileSize.Height).Clamp(0, GridSize - 1))
        End Function

        Public Function GetTileUnderMouse() As Vector2
            Dim AbsoluteMousePosition As Vector2 = MouseHandler.MousePosition.ToVector2
            Dim RelativeMousePosition As Vector2 = New Vector2(Clamp(AbsoluteMousePosition.X - BoardOrigin.X, 0, BoardSize.Width), Clamp(AbsoluteMousePosition.Y - BoardOrigin.Y, 0, BoardSize.Height))
            Return New Vector2(CInt(Math.Floor(RelativeMousePosition.X / TileSize.Width).Clamp(0, GridSize - 1)), CInt(Math.Floor(RelativeMousePosition.Y / TileSize.Height).Clamp(0, GridSize - 1)))
        End Function

        Public Function GetLevelData(ByVal LevelNumber As Integer) As List(Of Integer)
            Select Case LevelNumber
                Case 1
                    Dim chance As Integer = CInt(Random.Next(0, 5))
                    Select Case chance
                        Case 0
                            Return {3, 1, 6}.ToList
                        Case 1
                            Return {0, 3, 6}.ToList
                        Case 2
                            Return {5, 0, 6}.ToList
                        Case 3
                            Return {2, 2, 6}.ToList
                        Case 4
                            Return {4, 1, 6}.ToList
                    End Select
                Case 2
                    Dim chance As Integer = CInt(Random.Next(0, 5))
                    Select Case chance
                        Case 0
                            Return {1, 3, 7}.ToList
                        Case 1
                            Return {6, 0, 7}.ToList
                        Case 2
                            Return {3, 2, 7}.ToList
                        Case 3
                            Return {0, 4, 7}.ToList
                        Case 4
                            Return {5, 1, 7}.ToList
                    End Select
                Case 3
                    Dim chance As Integer = CInt(Random.Next(0, 5))
                    Select Case chance
                        Case 0
                            Return {2, 3, 8}.ToList
                        Case 1
                            Return {7, 0, 8}.ToList
                        Case 2
                            Return {4, 2, 8}.ToList
                        Case 3
                            Return {1, 4, 8}.ToList
                        Case 4
                            Return {6, 1, 8}.ToList
                    End Select
                Case 4
                    Dim chance As Integer = CInt(Random.Next(0, 5))
                    Select Case chance
                        Case 0
                            Return {3, 3, 8}.ToList
                        Case 1
                            Return {0, 5, 8}.ToList
                        Case 2
                            Return {8, 0, 10}.ToList
                        Case 3
                            Return {5, 2, 10}.ToList
                        Case 4
                            Return {2, 4, 10}.ToList
                    End Select
                Case 5
                    Dim chance As Integer = CInt(Random.Next(0, 5))
                    Select Case chance
                        Case 0
                            Return {7, 1, 10}.ToList
                        Case 1
                            Return {4, 3, 10}.ToList
                        Case 2
                            Return {1, 5, 10}.ToList
                        Case 3
                            Return {9, 0, 10}.ToList
                        Case 4
                            Return {6, 2, 10}.ToList
                    End Select
                Case 6
                    Dim chance As Integer = CInt(Random.Next(0, 5))
                    Select Case chance
                        Case 0
                            Return {3, 4, 10}.ToList
                        Case 1
                            Return {0, 6, 10}.ToList
                        Case 2
                            Return {8, 1, 10}.ToList
                        Case 3
                            Return {5, 3, 10}.ToList
                        Case 4
                            Return {2, 5, 10}.ToList
                    End Select
                Case 7
                    Dim chance As Integer = CInt(Random.Next(0, 5))
                    Select Case chance
                        Case 0
                            Return {7, 2, 10}.ToList
                        Case 1
                            Return {4, 4, 10}.ToList
                        Case 2
                            Return {1, 6, 13}.ToList
                        Case 3
                            Return {9, 1, 13}.ToList
                        Case 4
                            Return {6, 3, 10}.ToList
                    End Select
                Case 8
                    Dim chance As Integer = CInt(Random.Next(0, 5))
                    Select Case chance
                        Case 0
                            Return {0, 7, 10}.ToList
                        Case 1
                            Return {8, 2, 10}.ToList
                        Case 2
                            Return {5, 4, 10}.ToList
                        Case 3
                            Return {2, 6, 10}.ToList
                        Case 4
                            Return {7, 3, 10}.ToList
                    End Select
                Case Else
                    Return Nothing
            End Select

            Return Nothing
        End Function

        Protected Overrides Function GetFontRenderer() As SpriteBatch
            If IsCurrentScreen() AndAlso _interfaceFade + 0.01F >= 1.0F Then
                Return FontRenderer
            Else
                Return SpriteBatch
            End If
        End Function

        Public Overrides Sub SizeChanged()
            GameOrigin = New Vector2(CInt(windowSize.Width / 2 - GameSize.Width / 2), CInt(windowSize.Height / 2 - _screenTransitionY))
            BoardOrigin = New Vector2(GameOrigin.X + 32, GameOrigin.Y + 160)
            BoardCursorDestination = GetCursorOffset(0, 0)
            BoardCursorPosition = GetCursorOffset(0, 0)
        End Sub

        Public Sub UpdateTiles()
            For _row = 0 To GridSize - 1
                For _column = 0 To GridSize - 1
                    Dim _tile As Tile = Board(_row)(_column)
                    _tile.Update()
                Next
            Next
        End Sub
        Public Overrides Sub Update()

            ChooseBox.Update()
            If ChooseBox.Showing = False Then
                TextBox.Update()
            End If

            If ChooseBox.Showing = False AndAlso TextBox.Showing = False Then
                If Delay > 0 Then
                    Delay -= 1
                    If Delay <= 0 Then
                        Delay = 0
                    End If
                End If
            End If

            If Board IsNot Nothing Then
                UpdateTiles()
            End If
            If Delay = 0 Then
                If ChooseBox.Showing = False AndAlso TextBox.Showing = False Then
                    If GameState = States.Game Or GameState = States.Memo Then
                        'Moving the cursor between Tiles on the board
                        If Controls.Up(True, True, False) Then
                            If BoardCursorDestination.Y > GetCursorOffset(Nothing, 0).Y Then
                                BoardCursorDestination.Y -= GetCursorOffset(Nothing, 1).Y
                            Else
                                BoardCursorDestination.Y = GetCursorOffset(Nothing, 4).Y
                            End If
                        End If

                        If Controls.Down(True, True, False) = True Then
                            If BoardCursorDestination.Y < GetCursorOffset(Nothing, 4).Y Then
                                BoardCursorDestination.Y += GetCursorOffset(Nothing, 1).Y
                            Else
                                BoardCursorDestination.Y = GetCursorOffset(Nothing, 0).Y
                            End If
                        End If

                        If Controls.Left(True, True, False) = True Then
                            If BoardCursorDestination.X > GetCursorOffset(0, Nothing).X Then
                                BoardCursorDestination.X -= GetCursorOffset(1, Nothing).X
                            Else
                                BoardCursorDestination.X = GetCursorOffset(4, Nothing).X
                            End If
                        End If

                        If Controls.Right(True, True, False) = True Then
                            If BoardCursorDestination.X < GetCursorOffset(4, Nothing).X Then
                                BoardCursorDestination.X += GetCursorOffset(1, Nothing).X
                            Else
                                BoardCursorDestination.X = GetCursorOffset(0, Nothing).X
                            End If
                        End If

                        'Animation of Cursor
                        BoardCursorPosition.X = MathHelper.Lerp(BoardCursorPosition.X, BoardCursorDestination.X, 0.6F)
                        BoardCursorPosition.Y = MathHelper.Lerp(BoardCursorPosition.Y, BoardCursorDestination.Y, 0.6F)

                    Else
                        'Reset cursor position between levels
                        BoardCursorDestination = GetCursorOffset(0, 0)
                        BoardCursorPosition = GetCursorOffset(0, 0)
                    End If

                    'Switching between Game and Memo GameStates (Keys & GamePad)
                    If KeyBoardHandler.KeyPressed(KeyBindings.RunKey) Or ControllerHandler.ButtonPressed(Buttons.X) Then
                        If GameState = States.Game Then
                            GameState = States.Memo
                        ElseIf GameState = States.Memo Then
                            GameState = States.Game
                        End If
                    End If

                    'Switching between Game and Memo GameStates (Mouse)
                    Dim ButtonRectangle As Rectangle = New Rectangle(CInt(BoardOrigin.X + BoardSize.Width + TileSize.Width / 4), CInt(BoardOrigin.Y), MemoMenuSize.Width, MemoMenuSize.Height)
                    If Controls.Accept(True, False, False) = True AndAlso MouseHandler.IsInRectangle(ButtonRectangle) AndAlso Delay = 0 Then
                        If GameState = States.Game Then
                            GameState = States.Memo
                        ElseIf GameState = States.Memo Then
                            GameState = States.Game
                        End If
                    End If

                    If GameState = States.Memo Then
                        'Animate opening the Memo window
                        If MemoMenuX < MemoMenuSize.Width + TileSize.Width / 4 Then
                            MemoMenuX = MathHelper.Lerp(CSng(MemoMenuSize.Width + TileSize.Width / 4), MemoMenuX, 0.9F)
                            If MemoMenuX >= MemoMenuSize.Width + TileSize.Width / 4 Then
                                MemoMenuX = CInt(MemoMenuSize.Width + TileSize.Width / 4)
                            End If
                        End If

                        'Cycling through the 4 Memo types (Voltorb, One, Two, Three)
                        If Controls.Left(True, False, True, False, False, False) = True OrElse ControllerHandler.ButtonPressed(Buttons.LeftShoulder) Then
                            MemoIndex -= 1
                            If MemoIndex < 0 Then
                                MemoIndex = 3
                            End If
                        End If
                        If Controls.Right(True, False, True, False, False, False) = True OrElse ControllerHandler.ButtonPressed(Buttons.RightShoulder) Then
                            MemoIndex += 1
                            If MemoIndex > 3 Then
                                MemoIndex = 0
                            End If
                        End If

                        'Set the Memo type to the one under the mouse
                        Dim MemoMenuRectangle As New Rectangle(CInt(BoardOrigin.X + BoardSize.Width - MemoMenuSize.Width + MemoMenuX), CInt(BoardOrigin.Y + MemoMenuSize.Height + TileSize.Height / 2), MemoMenuSize.Width, MemoMenuSize.Height)
                        If Controls.Accept(True, False, False) = True Then
                            If MouseHandler.IsInRectangle(New Rectangle(MemoMenuRectangle.X, MemoMenuRectangle.Y, CInt(MemoMenuRectangle.Width / 2), CInt(MemoMenuRectangle.Height / 2))) = True Then
                                'Voltorb
                                MemoIndex = 0
                            End If
                            If MouseHandler.IsInRectangle(New Rectangle(MemoMenuRectangle.X + CInt(MemoMenuRectangle.Width / 2), MemoMenuRectangle.Y, CInt(MemoMenuRectangle.Width / 2), CInt(MemoMenuRectangle.Height / 2))) = True Then
                                'One
                                MemoIndex = 1
                            End If
                            If MouseHandler.IsInRectangle(New Rectangle(MemoMenuRectangle.X, MemoMenuRectangle.Y + CInt(MemoMenuRectangle.Height / 2), CInt(MemoMenuRectangle.Width / 2), CInt(MemoMenuRectangle.Height / 2))) = True Then
                                'Two
                                MemoIndex = 2
                            End If
                            If MouseHandler.IsInRectangle(New Rectangle(MemoMenuRectangle.X + CInt(MemoMenuRectangle.Width / 2), MemoMenuRectangle.Y + CInt(MemoMenuRectangle.Height / 2), CInt(MemoMenuRectangle.Width / 2), CInt(MemoMenuRectangle.Height / 2))) = True Then
                                'Three
                                MemoIndex = 3
                            End If
                        End If
                    Else
                        'Animate Closing the Memo window
                        If MemoMenuX > 0F Then
                            MemoMenuX = MathHelper.Lerp(0F, MemoMenuX, 0.9F)
                            If MemoMenuX <= 0F Then
                                MemoMenuX = 0F
                            End If
                        End If
                    End If

                    Dim QuitQuestionText As String = "Do you want to stop~playing Voltorb Flip?%Yes|No%"

                    'Quiting Voltorb Flip
                    If Controls.Dismiss(False, True, True) AndAlso GameState = States.Game AndAlso Delay = 0 Then
                        TextBox.Show(QuitQuestionText)
                        GameState = States.QuitQuestion
                    End If

                    'Quiting Voltorb Flip using the mouse
                    Dim QuitButtonRectangle As New Rectangle(CInt(GameOrigin.X + 424), CInt(GameOrigin.Y + 448), 128, 56)
                    If Controls.Accept(True, False, False) AndAlso MouseHandler.IsInRectangle(QuitButtonRectangle) AndAlso GameState = States.Game AndAlso Delay = 0 Then
                        TextBox.Show(QuitQuestionText)
                        GameState = States.QuitQuestion
                    End If


                    If GameState = States.QuitQuestion Then
                        If ChooseBox.readyForResult = True Then
                            If ChooseBox.result = 0 Then
                                Quit()
                            Else
                                Delay = 15
                                GameState = States.Game
                            End If
                        End If
                    End If

                    'Flip currently selected Tile
                    If Controls.Accept(False, True, True) AndAlso GameState = States.Game AndAlso Delay = 0 Then
                        Dim CurrentTile As Vector2 = GetCurrentTile()
                        Board(CInt(CurrentTile.Y))(CInt(CurrentTile.X)).Flip()
                    End If

                    'Flip the Tile that the mouse is on
                    If Controls.Accept(True, False, False) AndAlso GameState = States.Game AndAlso MouseHandler.IsInRectangle(New Rectangle(CInt(BoardOrigin.X), CInt(BoardOrigin.Y), BoardSize.Width, BoardSize.Height)) AndAlso Delay = 0 Then
                        Dim TileUnderMouse As Vector2 = GetTileUnderMouse()
                        BoardCursorDestination = GetCursorOffset(CInt(TileUnderMouse.X), CInt(TileUnderMouse.Y))
                        Board(CInt(TileUnderMouse.Y))(CInt(TileUnderMouse.X)).Flip()
                    End If

                    'Adding currently selected Memo to currently selected Tile
                    If Controls.Accept(False, True, True) AndAlso GameState = States.Memo AndAlso Board(CInt(GetCurrentTile.Y))(CInt(GetCurrentTile.X)).Flipped = False AndAlso Delay = 0 Then
                        Board(CInt(GetCurrentTile.Y))(CInt(GetCurrentTile.X)).SetMemo(MemoIndex, True)
                    End If

                    'Adding currently selected Memo to Tile that the mouse is on
                    If Controls.Accept(True, False, False) AndAlso GameState = States.Memo AndAlso MouseHandler.IsInRectangle(New Rectangle(CInt(BoardOrigin.X), CInt(BoardOrigin.Y), BoardSize.Width, BoardSize.Height)) AndAlso Delay = 0 Then
                        Dim TileUnderMouse As Vector2 = GetTileUnderMouse()
                        BoardCursorDestination = GetCursorOffset(CInt(TileUnderMouse.X), CInt(TileUnderMouse.Y))
                        If Board(CInt(GetTileUnderMouse.Y))(CInt(GetTileUnderMouse.X)).Flipped = False Then
                            Board(CInt(TileUnderMouse.Y))(CInt(TileUnderMouse.X)).SetMemo(MemoIndex, True)
                        End If
                    End If

                    'Removing currently selected Memo from currently selected Tile
                    If Controls.Dismiss(False, True, True) AndAlso GameState = States.Memo AndAlso Board(CInt(GetCurrentTile.Y))(CInt(GetCurrentTile.X)).Flipped = False AndAlso Delay = 0 Then
                        Board(CInt(GetCurrentTile.Y))(CInt(GetCurrentTile.X)).SetMemo(MemoIndex, False)
                    End If

                    'Removing currently selected Memo from Tile that the mouse is on
                    If Controls.Dismiss(True, False, False) AndAlso GameState = States.Memo AndAlso MouseHandler.IsInRectangle(New Rectangle(CInt(BoardOrigin.X), CInt(BoardOrigin.Y), BoardSize.Width, BoardSize.Height)) AndAlso Delay = 0 Then
                        Dim TileUnderMouse As Vector2 = GetTileUnderMouse()
                        BoardCursorDestination = GetCursorOffset(CInt(TileUnderMouse.X), CInt(TileUnderMouse.Y))
                        If Board(CInt(GetTileUnderMouse.Y))(CInt(GetTileUnderMouse.X)).Flipped = False Then
                            Board(CInt(TileUnderMouse.Y))(CInt(TileUnderMouse.X)).SetMemo(MemoIndex, False)
                        End If
                    End If
                End If
            End If

            'Level complete!
            If CurrentCoins >= MaxCoins AndAlso GameState = States.Game Then
                Dim GameClearText = "Game clear! You received~" & CurrentCoins.ToString & " " & "Coins!"
                TextBox.Show(GameClearText)
                If Delay = 0 Then
                    PreviousLevel = CurrentLevel

                    TotalFlips += CurrentFlips
                    CurrentFlips = 0
                    ConsequentWins += 1

                    If ConsequentWins = 5 AndAlso TotalFlips >= 8 Then
                        CurrentLevel = MaxLevel + 1
                    Else
                        If CurrentLevel < MaxLevel + 1 Then
                            If CurrentLevel + 1 > MaxLevel Then
                                CurrentLevel = MaxLevel
                            Else
                                CurrentLevel += 1
                            End If
                        End If
                    End If

                    GameState = States.GameWon
                    Delay = 5
                End If
            End If

            'Completed the level
            If GameState = States.GameWon Then
                If Core.Player.Coins + TotalCoins > 50000 Then
                    TotalCoins = 50000 - Core.Player.Coins
                    CurrentCoins = 0
                    TextBox.Show("Your Coin Case can't fit~any more Coins!*You received~" & TotalCoins.ToString & " " & "Coins instead!")
                    Quit()
                Else
                    TotalCoins += CurrentCoins
                    CurrentCoins = 0
                End If

                'Flip all Tiles to reveal contents
                Dim ReadyAmount As Integer = 0
                For _row = 0 To GridSize - 1
                    For _column = 0 To GridSize - 1
                        Board(_row)(_column).Reveal()
                        If Board(_row)(_column).FlipProgress = 0 Then
                            ReadyAmount += 1
                        End If
                    Next
                Next

                If Controls.Accept = True AndAlso TextBox.Showing = False Then
                    If ReadyAmount = CInt(GridSize * GridSize) Then
                        GameState = States.FlipWon
                    End If
                End If
            End If

            'Revealed a Voltorb
            If GameState = States.GameLost Then

                CurrentCoins = 0

                'Flip all Tiles to reveal contents
                Dim ReadyAmount As Integer = 0
                For _row = 0 To GridSize - 1
                    For _column = 0 To GridSize - 1
                        Board(_row)(_column).Reveal()
                        If Board(_row)(_column).FlipProgress = 0 Then
                            ReadyAmount += 1
                        End If
                    Next
                Next

                If ReadyAmount = CInt(GridSize * GridSize) Then
                    If Controls.Accept = True AndAlso TextBox.Showing = False Then
                        PreviousLevel = CurrentLevel
                        If CurrentFlips < CurrentLevel Then
                            CurrentLevel = Math.Max(1, CurrentFlips)
                        Else
                            CurrentLevel = 1
                        End If
                        GameState = States.FlipLost
                        End If
                    End If
            End If

            'Increase Level, reset Tiles
            If GameState = States.FlipWon Then
                Dim ReadyAmount As Integer = 0
                For _row = 0 To GridSize - 1
                    For _column = 0 To GridSize - 1
                        Board(_row)(_column).Reset()
                        If Board(_row)(_column).FlipProgress = 0 Then
                            ReadyAmount += 1
                        End If
                    Next
                Next

                If ReadyAmount = CInt(GridSize * GridSize) Then
                    GameState = States.NewLevel
                End If
            End If

            'Drop Level, reset Tiles
            If GameState = States.FlipLost Then
                Dim ReadyAmount As Integer = 0
                For _row = 0 To GridSize - 1
                    For _column = 0 To GridSize - 1
                        Board(_row)(_column).Reset()
                        If Board(_row)(_column).FlipProgress = 0 Then
                            ReadyAmount += 1
                        End If
                    Next
                Next

                CurrentFlips = 0

                If ReadyAmount = CInt(GridSize * GridSize) Then
                    GameState = States.NewLevel
                End If
            End If

            'Prepare new Level
            If GameState = States.NewLevel Then
                If TextBox.Showing = False Then
                    Board = CreateBoard(CurrentLevel)
                    If CurrentLevel < PreviousLevel Then
                        TextBox.Show("Dropped to Game Lv." & " " & CurrentLevel & "!")
                    End If

                    If CurrentLevel = PreviousLevel Then
                        TextBox.Show("Ready to play Game Lv." & " " & CurrentLevel & "!")
                    End If

                    If CurrentLevel > PreviousLevel Then
                        TextBox.Show("Advanced to Game Lv." & " " & CurrentLevel & "!")
                    End If
                Else
                    Delay = 15
                    GameState = States.Game
                End If
            End If

            'Animation of opening/closing the window
            If GameState = States.Closing Then
                Dim ResultCoins As Integer = 0
                Dim AnimationCurrentCoins As Single = CurrentCoins

                If CurrentCoins > ResultCoins Then

                    CurrentCoins = 0
                End If

                If _interfaceFade > 0F Then
                    _interfaceFade = MathHelper.Lerp(0, _interfaceFade, 0.8F)
                    If _interfaceFade < 0F Then
                        _interfaceFade = 0F
                    End If
                End If
                If _screenTransitionY > 0 Then
                    _screenTransitionY = MathHelper.Lerp(0, _screenTransitionY, 0.8F)
                    If _screenTransitionY <= 0 Then
                        _screenTransitionY = 0
                    End If
                End If

                GameOrigin.Y = CInt(windowSize.Height / 2 - _screenTransitionY)
                BoardOrigin = New Vector2(GameOrigin.X + 32, GameOrigin.Y + 160)

                If _screenTransitionY <= 2.0F Then
                    SetScreen(PreScreen)
                End If
            Else
                Dim maxWindowHeight As Integer = CInt(GameSize.Height / 2)
                If _screenTransitionY < maxWindowHeight Then
                    _screenTransitionY = MathHelper.Lerp(maxWindowHeight, _screenTransitionY, 0.8F)
                    If _screenTransitionY >= maxWindowHeight - 0.8 Then
                        If GameState = States.Opening Then
                            GameState = States.NewLevel
                        End If
                        _screenTransitionY = maxWindowHeight
                    End If
                End If
                GameOrigin.Y = CInt(windowSize.Height / 2 - _screenTransitionY)
                BoardOrigin = New Vector2(GameOrigin.X + 32, GameOrigin.Y + 160)

                If _interfaceFade < 1.0F Then
                    _interfaceFade = MathHelper.Lerp(1, _interfaceFade, 0.95F)
                    If _interfaceFade = 1.0F Then
                        _interfaceFade = 1.0F
                    End If
                End If
            End If
        End Sub

        Public Sub Quit()
            If CurrentFlips < CurrentLevel Then
                CurrentLevel = Math.Max(1, CurrentFlips)
            Else
                CurrentLevel = 1
            End If
            PreviousLevel = CurrentLevel

            TextBox.Show("Game Over!~Dropped to Game Lv." & " " & CurrentLevel & "!")

            CurrentFlips = 0
            TotalFlips = 0

            CurrentCoins = 0

            GameState = States.Closing
        End Sub
    End Class


    Public Class Tile
        Public Enum Values
            Voltorb
            One
            Two
            Three
        End Enum
        Public Property Row As Integer = 0
        Public Property Column As Integer = 0
        Public Property Value As Integer = Tile.Values.Voltorb
        Public Property Flipped As Boolean = False
        Private Property MemoVoltorb As Boolean = False
        Private Property Memo1 As Boolean = False
        Private Property Memo2 As Boolean = False
        Private Property Memo3 As Boolean = False

        Private Property FlipWidth As Single = 1.0F
        Private Property Activated As Boolean = False
        Public Property FlipProgress As Integer = 0

        Public Sub Flip()
            If Flipped = False Then
                FlipProgress = 3
                If Value <> Values.Voltorb Then
                    VoltorbFlipScreen.CurrentFlips += 1
                End If
            End If
        End Sub

        Public Sub Reveal()
            If Flipped = False Then
                FlipProgress = 1
            End If
        End Sub
        Public Sub Reset()
            If Flipped = True Then
                FlipProgress = 1
                Activated = False
            End If
        End Sub

        Public Sub Draw()
            Dim mainBackgroundColor As Color = New Color(255, 255, 255)
            If VoltorbFlipScreen.GameState = VoltorbFlipScreen.States.Closing Or VoltorbFlipScreen.GameState = VoltorbFlipScreen.States.Opening Then
                mainBackgroundColor = New Color(255, 255, 255, CInt(255 * VoltorbFlipScreen._interfaceFade))
            End If

            Dim TileWidth = VoltorbFlipScreen.TileSize.Width
            Dim TileHeight = VoltorbFlipScreen.TileSize.Height

            If FlipProgress = 1 OrElse FlipProgress = 3 Then
                If FlipWidth > 0F Then
                    FlipWidth -= 0.1F
                End If
                If FlipWidth <= 0F Then
                    FlipWidth = 0F
                    If Flipped = False Then
                        SetMemo(0, False)
                        SetMemo(1, False)
                        SetMemo(2, False)
                        SetMemo(3, False)
                        Flipped = True
                    Else
                        Flipped = False
                    End If
                    FlipProgress += 1
                    End If
                End If
            If FlipProgress = 2 OrElse FlipProgress = 4 Then
                If FlipWidth < 1.0F Then
                    FlipWidth += 0.1F
                End If
                If FlipWidth >= 1.0F Then
                    FlipWidth = 1.0F
                    FlipProgress = 0
                End If
            End If

            'Draw Tile
            SpriteBatch.Draw(GetImage, New Rectangle(CInt(VoltorbFlipScreen.BoardOrigin.X + TileWidth * Column + (TileWidth - FlipWidth * TileWidth) / 2), CInt(VoltorbFlipScreen.BoardOrigin.Y + TileHeight * Row), CInt(TileWidth * FlipWidth), TileHeight), mainBackgroundColor)

            'Draw Memos
            If GetMemo(0) = True Then 'Voltorb
                SpriteBatch.Draw(TextureManager.GetTexture("VoltorbFlip\Tile_MemoIcons", New Rectangle(0, 0, 32, 32)), New Rectangle(CInt(VoltorbFlipScreen.BoardOrigin.X + TileWidth * Column + (TileWidth - FlipWidth * TileWidth)), CInt(VoltorbFlipScreen.BoardOrigin.Y + TileHeight * Row), CInt(TileWidth * FlipWidth), TileHeight), mainBackgroundColor)
            End If
            If GetMemo(1) = True Then 'x1
                SpriteBatch.Draw(TextureManager.GetTexture("VoltorbFlip\Tile_MemoIcons", New Rectangle(32, 0, 32, 32)), New Rectangle(CInt(VoltorbFlipScreen.BoardOrigin.X + TileWidth * Column + (TileWidth - FlipWidth * TileWidth)), CInt(VoltorbFlipScreen.BoardOrigin.Y + TileHeight * Row), CInt(TileWidth * FlipWidth), TileHeight), mainBackgroundColor)
            End If
            If GetMemo(2) = True Then 'x2
                SpriteBatch.Draw(TextureManager.GetTexture("VoltorbFlip\Tile_MemoIcons", New Rectangle(32 + 32, 0, 32, 32)), New Rectangle(CInt(VoltorbFlipScreen.BoardOrigin.X + TileWidth * Column + (TileWidth - FlipWidth * TileWidth)), CInt(VoltorbFlipScreen.BoardOrigin.Y + TileHeight * Row), CInt(TileWidth * FlipWidth), TileHeight), mainBackgroundColor)
            End If
            If GetMemo(3) = True Then 'x3
                SpriteBatch.Draw(TextureManager.GetTexture("VoltorbFlip\Tile_MemoIcons", New Rectangle(32 + 32 + 32, 0, 32, 32)), New Rectangle(CInt(VoltorbFlipScreen.BoardOrigin.X + TileWidth * Column + (TileWidth - FlipWidth * TileWidth)), CInt(VoltorbFlipScreen.BoardOrigin.Y + TileHeight * Row), CInt(TileWidth * FlipWidth), TileHeight), mainBackgroundColor)
            End If
        End Sub

        Public Sub Update()
            If FlipProgress <= 2 Then
                Activated = False
            Else
                If Flipped = True Then
                    If Activated = False Then
                        If Me.Value = Values.Voltorb Then
                            Screen.TextBox.Show("Oh no! You get 0 coins!")
                            VoltorbFlipScreen.GameState = VoltorbFlipScreen.States.GameLost
                        Else
                            If VoltorbFlipScreen.CurrentCoins = 0 Then
                                VoltorbFlipScreen.CurrentCoins = Me.Value
                            Else
                                VoltorbFlipScreen.CurrentCoins *= Me.Value
                            End If
                            Activated = True
                        End If
                    End If
                End If
            End If
        End Sub

        Public Function GetImage() As Texture2D
            If Flipped = True Then
                Return TextureManager.GetTexture("VoltorbFlip\Tile_Front", New Rectangle(Value * 32, 0, 32, 32))
            Else
                Return TextureManager.GetTexture("VoltorbFlip\Tile_Back", New Rectangle(0, 0, 32, 32))
            End If
        End Function

        Public Function GetMemo(ByVal MemoNumber As Integer) As Boolean
            Select Case MemoNumber
                Case 0
                    Return MemoVoltorb
                Case 1
                    Return Memo1
                Case 2
                    Return Memo2
                Case 3
                    Return Memo3
                Case Else
                    Return Nothing
            End Select
        End Function

        Public Sub SetMemo(ByVal MemoNumber As Integer, ByVal Value As Boolean)
            Select Case MemoNumber
                Case Tile.Values.Voltorb
                    MemoVoltorb = Value
                Case Tile.Values.One
                    Memo1 = Value
                Case Tile.Values.Two
                    Memo2 = Value
                Case Tile.Values.Three
                    Memo3 = Value
            End Select
        End Sub

        Public Sub New(ByVal Row As Integer, ByVal Column As Integer, ByVal Value As Integer, ByVal Flipped As Boolean)
            Me.Row = Row
            Me.Column = Column
            Me.Value = Value
            Me.Flipped = Flipped
        End Sub

    End Class

End Namespace