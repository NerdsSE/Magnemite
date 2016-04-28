Imports DevExpress.XtraTreeList.Nodes
Imports Microsoft.Win32.Registry

Public Class frmMain
    Private _localBase As String

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        For Each cadaLinha As String In My.Application.CommandLineArgs.ToArray
            montaTela(cadaLinha)
        Next
    End Sub

    Private Sub montaTela(localBase As String)
        _localBase = localBase

        TreeList1.Nodes.Clear()
        If System.IO.Directory.Exists(localBase) Then
            Dim lala As New System.IO.DirectoryInfo(localBase)

            Dim noPai As TreeListNode = TreeList1.AppendNode({lala.Name, lala}, DirectCast(Nothing, TreeListNode))
            noPai.ImageIndex = 0

            adicionaFilhos(lala, noPai)

            TreeList1.ExpandAll()
            btnVerifica_ItemClick(Nothing, Nothing)
        End If
    End Sub

    Private Sub adicionaFilhos(Diretorio As System.IO.DirectoryInfo, ByRef noPai As TreeListNode)
        For Each cadaDir As System.IO.DirectoryInfo In Diretorio.GetDirectories
            Dim novoPai As TreeListNode = TreeList1.AppendNode({cadaDir.Name, ""}, noPai)
            novoPai.ImageIndex = 0
            adicionaFilhos(cadaDir, novoPai)
        Next

        For Each cadaFile As System.IO.FileInfo In Diretorio.GetFiles
            Dim novoNo As TreeListNode = TreeList1.AppendNode({cadaFile.Name, cadaFile.FullName}, noPai)
            novoNo.ImageIndex = 1
            novoNo.SelectImageIndex = 1
        Next
    End Sub

    Private Sub btnSeleioncaDir_ItemClick(sender As Object, e As DevExpress.XtraBars.ItemClickEventArgs) Handles btnSeleioncaDir.ItemClick
        If FolderBrowserDialog1.ShowDialog Then
            montaTela(FolderBrowserDialog1.SelectedPath)
        End If
    End Sub

    Private Sub btnVerifica_ItemClick(sender As Object, e As DevExpress.XtraBars.ItemClickEventArgs) Handles btnVerifica.ItemClick
        TreeList1.Nodes(0).Checked = False
        verificaFilhos(TreeList1.Nodes(0))
    End Sub

    Private Sub verificaFilhos(noPai As TreeListNode)
        For Each cadaNo As TreeListNode In noPai.Nodes
            cadaNo.Checked = False
            If cadaNo.ImageIndex = 1 Then
                Dim arquivo As New System.IO.FileInfo(cadaNo.GetValue(colLocal))
                If arquivo.Extension.ToUpper.Equals(".LOG") Or _
                   arquivo.Name.ToUpper.StartsWith("ERRORS.LST.") Or _
                   arquivo.Name.ToUpper.Contains(".INF.") Then
                    cadaNo.Checked = True
                End If
                For Each cadaTipo As String In {".ASM.", ".PRT.", ".DRW."}
                    If arquivo.Name.ToUpper.Contains(cadaTipo) Then
                        Dim num As Integer = arquivo.Extension.Replace(".", "")
                        Dim novoNome As String = arquivo.FullName.Substring(0, arquivo.FullName.LastIndexOf(".") + 1) & (num + 1)
                        If System.IO.File.Exists(novoNome) Then
                            cadaNo.Checked = True
                        End If
                    End If
                Next
            End If
            verificaFilhos(cadaNo)
        Next
    End Sub

   
    Private Sub btnExcluir_ItemClick(sender As Object, e As DevExpress.XtraBars.ItemClickEventArgs) Handles btnExcluir.ItemClick
        excluiItens(TreeList1.Nodes(0))
        montaTela(_localBase)
    End Sub

    Private Sub excluiItens(noPai As TreeListNode)
        For Each cadaNo As TreeListNode In noPai.Nodes
            If cadaNo.ImageIndex = 1 Then
                If cadaNo.Checked Then
                    System.IO.File.Delete(cadaNo.GetValue(colLocal))
                End If
            End If
            excluiItens(cadaNo)
        Next
    End Sub

    Private Sub btnRegistro_ItemClick(sender As Object, e As DevExpress.XtraBars.ItemClickEventArgs) Handles btnRegistro.ItemClick
        Dim appData As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
        If Not System.IO.Directory.Exists(appData & "\Magnemite\") Then
            System.IO.Directory.CreateDirectory(appData & "\Magnemite\")
        End If
        Dim dirPai As New System.IO.DirectoryInfo("\\brsum-fs01\Company\BU - Comum\Publisher\Magnemite\Application Files\Magnemite_1_0_0_0\")
        For Each cadaArquivo As System.IO.FileInfo In dirPai.GetFiles("*.dll")
            FileCopy(cadaArquivo.FullName, appData & "\Magnemite\" & cadaArquivo.Name)
        Next

        For Each cadaArquivo As System.IO.FileInfo In dirPai.GetFiles("Magnemite.*")
            FileCopy(cadaArquivo.FullName, appData & "\Magnemite\" & cadaArquivo.Name)
        Next

        Dim reg As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey("Directory\shell\Magnemite")
        If reg Is Nothing Then
            reg = Microsoft.Win32.Registry.ClassesRoot.CreateSubKey("Directory\shell\Magnemite")
            reg.SetValue("Icon", appData & "\Magnemite\magnemite.ico", Microsoft.Win32.RegistryValueKind.String)
            reg.SetValue("", "Limpar arquivos com Magnemite")
            reg = reg.CreateSubKey("command")
            reg.SetValue("", appData & "\Magnemite\magnemite.exe ""%1""")
            reg.Close()
        End If
    End Sub
End Class