Module modTree
    ''' <summary>
    ''' Positions all nodes in the tree starting from the root
    ''' </summary>
    Public Sub LayoutTree(node As WNode, centerX As Integer, y As Integer, xOffset As Integer, yOffset As Integer)
        If node Is Nothing Then Return

        ' Position the current node
        node.Position = New Point(centerX - (node.Width \ 2), y)

        ' Get child count and calculate total width required
        Dim childCount As Integer = node.Children.Count
        If childCount = 0 Then Return ' No children, no further layout needed

        ' Calculate total width needed for all children
        Dim totalChildWidth As Integer = 0
        For Each child In node.Children
            totalChildWidth += child.Width
        Next
        totalChildWidth += (childCount - 1) * xOffset ' Add spacing between children

        ' Determine starting X position for first child
        Dim startX As Integer = centerX - (totalChildWidth \ 2)

        ' Layout children
        For Each child In node.Children
            Dim childCenterX As Integer = startX + (child.Width \ 2)
            LayoutTree(child, childCenterX, y + yOffset, xOffset, yOffset)
            startX += child.Width + xOffset
        Next
    End Sub

    ''' <summary>
    ''' Adjusts tree to prevent node overlaps
    ''' </summary>
    Public Sub AdjustOffsets(node As WNode, minOffset As Integer)
        If node Is Nothing OrElse node.Children.Count <= 1 Then Return

        ' First, recursively adjust each child's subtree
        For Each child In node.Children
            AdjustOffsets(child, minOffset)
        Next

        ' Then adjust siblings at this level
        Dim needsAnotherPass As Boolean
        Do
            needsAnotherPass = False
            For i = 1 To node.Children.Count - 1
                Dim leftChild = node.Children(i - 1)
                Dim rightChild = node.Children(i)

                ' Calculate the rightmost edge of the left subtree
                Dim leftSubtreeRightEdge = GetRightmostEdge(leftChild)

                ' Calculate the leftmost edge of the right subtree
                Dim rightSubtreeLeftEdge = GetLeftmostEdge(rightChild)

                ' Check if there's an overlap plus minimum desired spacing
                Dim overlap = (leftSubtreeRightEdge + minOffset) - rightSubtreeLeftEdge

                If overlap > 0 Then
                    ' Shift the right subtree
                    ShiftSubtree(rightChild, overlap)
                    needsAnotherPass = True ' We made a change, so we might need another pass
                End If
            Next
        Loop While needsAnotherPass

        ' Center parent above children if needed
        CenterParentAboveChildren(node)
    End Sub

    ''' <summary>
    ''' Gets the rightmost edge (x-coordinate) of a node and its subtree
    ''' </summary>
    Private Function GetRightmostEdge(node As WNode) As Integer
        If node Is Nothing Then Return 0

        Dim nodeRightEdge = node.Position.X + node.Width

        ' Check if any child extends beyond this node
        For Each child In node.Children
            Dim childRightEdge = GetRightmostEdge(child)
            If childRightEdge > nodeRightEdge Then
                nodeRightEdge = childRightEdge
            End If
        Next

        Return nodeRightEdge
    End Function

    ''' <summary>
    ''' Gets the leftmost edge (x-coordinate) of a node and its subtree
    ''' </summary>
    Private Function GetLeftmostEdge(node As WNode) As Integer
        If node Is Nothing Then Return Integer.MaxValue

        Dim nodeLeftEdge = node.Position.X

        ' Check if any child extends beyond this node
        For Each child In node.Children
            Dim childLeftEdge = GetLeftmostEdge(child)
            If childLeftEdge < nodeLeftEdge Then
                nodeLeftEdge = childLeftEdge
            End If
        Next

        Return nodeLeftEdge
    End Function

    ''' <summary>
    ''' Centers a parent node above its children
    ''' </summary>
    Private Sub CenterParentAboveChildren(node As WNode)
        If node Is Nothing OrElse node.Children.Count = 0 Then Return

        Dim firstChild = node.Children.First()
        Dim lastChild = node.Children.Last()

        ' Calculate the center point between the first and last child
        Dim leftEdge = GetLeftmostEdge(firstChild)
        Dim rightEdge = GetRightmostEdge(lastChild)
        Dim midpoint = leftEdge + ((rightEdge - leftEdge) \ 2)

        ' Center the parent node above this midpoint
        node.Position = New Point(midpoint - (node.Width \ 2), node.Position.Y)
    End Sub

    ''' <summary>
    ''' Shifts a node and its entire subtree by the specified amount
    ''' </summary>
    Private Sub ShiftSubtree(node As WNode, shiftAmount As Integer)
        If node Is Nothing Then Return

        ' Shift the current node
        node.Position = New Point(node.Position.X + shiftAmount, node.Position.Y)

        ' Shift all children recursively
        For Each child In node.Children
            ShiftSubtree(child, shiftAmount)
        Next
    End Sub

    ''' <summary>
    ''' Ensures the entire tree is visible within the specified bounds
    ''' </summary>
    Public Sub EnsureTreeInBounds(root As WNode, minX As Integer, minY As Integer,
                                 maxWidth As Integer, maxHeight As Integer)
        If root Is Nothing Then Return

        ' Get the bounds of the entire tree
        Dim leftEdge = GetLeftmostEdge(root)
        Dim rightEdge = GetRightmostEdge(root)
        Dim treeWidth = rightEdge - leftEdge

        ' If tree is wider than available space, scale down
        If treeWidth > maxWidth Then
            ' TODO: Implement scaling logic if needed
        End If

        ' If tree is outside left boundary, shift right
        If leftEdge < minX Then
            ShiftSubtree(root, minX - leftEdge)
        End If

        ' If tree is outside right boundary, shift left
        Dim rightBoundary = minX + maxWidth
        If rightEdge > rightBoundary Then
            ShiftSubtree(root, rightBoundary - rightEdge)
        End If
    End Sub
End Module