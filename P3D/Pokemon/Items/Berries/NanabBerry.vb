Namespace Items.Berries

    <Item(2017, "Nanab")>
    Public Class NanabBerry

        Inherits Berry

        Public Sub New()
            MyBase.New(3600, "Pokéblock ingredient. Plant in loamy soil to grow Nanab.", "7.7cm", "Very Hard", 2, 3)

            Me.Spicy = 0
            Me.Dry = 0
            Me.Sweet = 10
            Me.Bitter = 10
            Me.Sour = 0

            Me.Type = Element.Types.Water
            Me.Power = 90
            Me.JuiceColor = "pink"
            Me.JuiceGroup = 1
        End Sub

    End Class

End Namespace
