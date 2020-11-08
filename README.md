# RegisterAndLogin

## Created with  .NET Core Identity

Please edit the following pages.

##### appsettings.json

    "EmailStmp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "EnableSSL": true,
    "Email": "Please enter your gmail account",
    "Password": "Please enter your gmail account password"
    }

##### Startup.cs

    services.AddDbContext<UserAppContext>(options => options.UseSqlServer
    ("Server=SERVER NAME;Database=UserDb; Integrated Security=SSPI; MultipleActiveResultSets=True;"));



You can log in with the following user information.

Email: admin@test.com -
Password: Test_137
