# PwnedPasswords
A small .NET Core program to check if a password has been leaked by using Have I Been Pwned.


## Usage
Note: The program is currently tested for Windows and Linux, but should work on Mac. See [this page](https://github.com/dotnet/core/blob/master/release-notes/3.1/3.1-supported-os.md) for all supported OS.

Each password must be separated by space. If the password contains spaces, it must be surrounded with quotation marks, e.g. "Password With Spaces".
 
There is no limit to how many passwords that can be checked at a time, however the server might rate-limit the requests. 

### Windows
Open a command line in the same directory as the `pwnedpasswords.exe` file and enter:
```batchfile
pwnedpasswords.exe [password1] [password2] [...]
```

#### Example
For example, typing:
```batchfile
pwnedpasswords.exe password
```
Will output:
```batchfile
This password has been pwned, and it has been seen 3 730 471 times!
Pwned Hash: 5BAA61E4C9B93F3F0682250B6CF8331B7EE68FD8
Pwned Password: password
```

Currently the program requires passwords to be input as launch argument, which means it won't do anything if nothing is specified. 

## To-Do
- [x] Check multiple passwords at a time
- [x] Allow piping

## Data source
This project uses [Have I Been Pwned](https://haveibeenpwned.com) to check if a password has leaked.
