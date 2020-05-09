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

It is also possible to redirect a text file to the program, or piping a command to the program:
#### Redirecting (passing a text file to the program) Note: One password per line in the text document.
```batchfile
pwnedpasswords.exe < *.txt
```
This allows an easy way to check a larger list of passwords. 


#### Piping (passing the output of another program or a command to the program)
```batchfile
echo [password]| pwnedpasswords.exe
```
Note: When using `echo`, omit the space after the password, otherwise it will be considered as part of the password.

#### Examples
##### Typing...
```batchfile
pwnedpasswords.exe password
```
Will output:
```batchfile
This password has been pwned, and it has been seen 3 730 471 times!
Pwned Hash: 5BAA61E4C9B93F3F0682250B6CF8331B7EE68FD8
Pwned Password: password
```

##### Typing... (pwd.txt contains password1 and password2 on two separate lines)
```batchfile
pwnedpasswords.exe < pwd.txt
```
Will output:
```batchfile
This password has been pwned, and it has been seen 2 413 945 times!
Pwned Hash: E38AD214943DAAD1D64C102FAEC29DE4AFE9DA3D
Pwned Password: password1

This password has been pwned, and it has been seen 185 178 times!
Pwned Hash: 2AA60A8FF7FCD473D321E0146AFD9E26DF395147
Pwned Password: password2
```

Currently the program requires passwords to be input at run-time, which means it won't do anything if nothing is specified. 

## To-Do
- [x] Check multiple passwords at a time
- [x] Allow piping
- [ ] Ability to start without specifying passwords at run-time

## Data source
This project uses [Have I Been Pwned](https://haveibeenpwned.com) to check if a password has leaked.
