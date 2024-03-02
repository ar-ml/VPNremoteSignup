# To enable basic AES encryption for usernames and passwords in the config file, do the following:

- Make sure in appsettings.json encryption is enabled:
"ApplicationSettings": {
  "UseEncryption": true
},

- create a fresh AES key. it must be a 32 characters long = 256-bit AES key
You can use OpenSSL  for it or for our purpose it can just be any string with 32 characters.
You can use Keepass or any other password generator online to generate the 32-characters-key.

- In program.cs, line 34 add your static key:
static string Key => "add key here";

Now you want to encrypt your email login credentials and VPN usernames and passwords and add then add the encrypted strings to appsettings.json.
For encrypting you can use the EncryptText tool that comes with the project:

# If you want to build an Executable EncryptText.exe from sourcecode, do the following:
- Open Visual Studio
- Open Project folder and select VPNremoteSignup.sln
- Make a build of EncryptText.csproj by selecting it from the dropdown and pressing green play button in visual studio

# If you already have EncryptText.exe, you can use it like this:
- open cmd.exe
- navigate to the right folder, for example: cd D:\\gitrepo\VPNremoteSignup\src\EncryptText\obj\Debug
- execute EncryptText.exe with attributes, for example like this:
EncryptText.exe doqya80QeTBft210Vvp0uDGvSgd76Dfn "This is a text"
- Of course you have to use your own key for encrypting it. 
- Instead "This is a text" you have to paste the username or password you want to encrypt.
- After encrypting the credentials, paste the encrypted strings into appsettings.json.
