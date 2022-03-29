Imports System.IO
Imports ExcelDataReader

Public Class Form1
    Dim tables As DataTableCollection
    Private Sub btnBrowse_Click(sender As Object, e As EventArgs) Handles btnBrowse.Click
        Using openFileDialog As OpenFileDialog = New OpenFileDialog() With {.Filter = "(*.xlsx;*.xsl)|*.xlsx;*.xls"}
            If openFileDialog.ShowDialog() = DialogResult.OK Then
                txtFileName.Text = openFileDialog.FileName
                Using stream = File.Open(openFileDialog.FileName, FileMode.Open, FileAccess.Read)
                    Using reader As IExcelDataReader = ExcelReaderFactory.CreateReader(stream)
                        Dim result As DataSet = reader.AsDataSet(New ExcelDataSetConfiguration() With {.ConfigureDataTable = Function(__) New ExcelDataTableConfiguration() With {.UseHeaderRow = True}})
                        tables = result.Tables
                        For Each table As DataTable In tables
                            grdExcelTable.DataSource = table
                        Next
                    End Using
                End Using
            End If
        End Using
    End Sub

    Private Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        If grdExcelTable.RowCount < 1 Then
            MsgBox("Must have a loaded table")
            Exit Sub
        End If

        Dim itemName As String
        Dim itemPrice As Decimal
        Dim itemQuantity As Integer

        Try
            itemName = txtItem.Text
            itemQuantity = Integer.Parse(txtQuantity.Text)
            If String.IsNullOrEmpty(txtPrice.Text) = False Then
                itemPrice = Decimal.Parse(txtPrice.Text)
            End If
        Catch ex As Exception
            MsgBox("Please check form values")
            Exit Sub
        End Try

        Dim err As String = ValidateAddItemForm(itemName, itemQuantity, itemPrice)
        If err <> "" Then
            MsgBox(err)
            Exit Sub
        End If

        'Check rows for existing item
        For Each row As DataGridViewRow In grdExcelTable.Rows
            If row.Cells(0).Value IsNot DBNull.Value Then
                If row.Cells(0).Value = itemName Then
                    row.Cells(1).Value += itemQuantity
                    If String.IsNullOrEmpty(txtPrice.Text) = False Then
                        row.Cells(2).Value = itemPrice
                    End If
                    clearAddItemForm()
                    Exit Sub
                End If
            End If
            If row.Cells(0).Value Is DBNull.Value Then
                'Price must be set for new items
                If String.IsNullOrEmpty(txtPrice.Text) Then
                    MsgBox("You must set a price for new items")
                    Exit Sub
                End If
                row.Cells(0).Value = itemName
                row.Cells(1).Value = itemQuantity
                row.Cells(2).Value = itemPrice
                clearAddItemForm()
                Exit Sub
            End If
        Next
    End Sub

    Private Function validateAddItemForm(name As String, quantity As Integer, price As Decimal)
        Dim errorMessage As String = ""

        If name.Length > 50 Or name.Length = 0 Then
            errorMessage = "Name is too long or too short"
        End If

        If quantity >= 100000 Then
            errorMessage = "Please enter a quantity below 100,000"
        End If

        If price >= 100000 Then
            errorMessage = "Please enter a price below 100,000"
        End If

        Return errorMessage
    End Function

    Private Sub clearAddItemForm()
        txtItem.Text = ""
        txtPrice.Text = ""
        txtQuantity.Text = 0
    End Sub
End Class
