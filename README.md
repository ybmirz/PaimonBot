<img src="PaimonBot/Resources/Images/Icon_Paimon.jpg" align="right" width="85px"/>

# PaimonBot~
The best Discord bot partner for your Genshin Impact journeys! To use PaimonBot's  main features such as Resin and Realm Currency Tracking, or Parametric Gadget reminder, you'd need to create a profile by doing `p~account create` and it should start a prompt for you to answer and fill in information. There's **3** main command categories:

**NOTICE:** PaimonBot is still <span style="text-decoration: underline">under development</span> and is currently **not deployed**.

* **Account Commands**
    1. `p~account` - Shows you a dashboard embed of your current profile information.
    2. `p~account create` - Creates an account, if already exists, will overwrite.
    3. `p~account update [fieldname]` - Updates a specific field/data of the user's profile.
    4. `p~account delete` - Deletes the account.

   **Note**: An account is attributed to a **single** Discord User ID, so sadly only one tracker per Discord user.
* **Resin Commands**
    1. `p~resin` - Shows the user's current resin, if they do exist in the database.
    2. `p~resin set [amount]` - Sets the user's current resin amount. Creates an account if user does not exist.
    3. `p~resin reset` - Resets the user's current resin amount, directly to 0.
    4. `p~resin remind [amount]` - Sets PaimonBot to remind the user through DM at a specific resin amount.

   **Note**: Resin commands utilizes the same account as Account Cmds, however has a few of the fields on null or not-usable/not-shown.
* More commands coming soon such as **Material Location, Artifact, Weapon and Character Information, Daily To-Dos and Reminder commands**. 

**Notice:** More commands are to be added into the readme, once they have been coded and committed into this repository. PaimonBot currently uses [mongodb](https://www.mongodb.com/) as a database, [DSharpPlus](https://dsharpplus.github.io/) as its framework, and [Serilog](https://serilog.net/) for its logging.

## Contribution
If you'd like to contribute to the development of PaimonBot, please do **not** start contributing as of now, as it is still very much in early development, instead please do contact me through Discord: [xeno#3125](https://discord.com/users/574558925224017920). 
Would love help solving this current `StackOverflow` error :<

## License
[MIT](https://choosealicense.com/licenses/mit/)
