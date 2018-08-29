# Steps to run this app and use it in Microsoft Teams

1. Begin your tunneling service to get an https endpoint (we recommend [ngrok](https://ngrok.com/)).

	* Open a new **Command Prompt** window. 

	* Change to the directory that contains the ngrok.exe application. 

	* Run the command `ngrok http [port] --host-header=localhost` (you'll need the https endpoint for the bot registration) e.g.<br>
		```
		ngrok http 3979 --host-header=localhost
		```

	* The ngrok application will fill the entire prompt window. Make note of the Forwarding address using https. This address is required in the next step. 

	* Minimize the ngrok Command Prompt window. It must remain running to tunnel properly but can be kept in the background.

2. Register a new bot (or update an existing one) with Bot Framework by using the https endpoint started by ngrok and the extension "/api/messages" as the full endpoint for the bot's "Messaging endpoint". e.g. "https://####abcd.ngrok.io/api/messages" - Bot registration can be done through the [App Studio for Microsoft Teams](https://docs.microsoft.com/en-us/microsoftteams/platform/get-started/get-started-app-studio) app.

    > **NOTE**: When you create your bot you will create an App ID and App password - make sure you keep these for later.

3. Your project needs to run with a configuration that matches your registered bot's configuration. To do this, you will need to update the Web.config file:

	* In Visual Studio, open the Web.config file. Locate the `<appSettings>` section. 
 
	* Enter the MicrosoftAppId. The MicrosoftAppId is the app ID from the **Configuration** section of the bot registration. 
 
	* Enter the MicrosoftAppPassword. The MicrosoftAppPassword is the auto-generated app password displayed in the pop-up during bot registration.

	Here is an example for reference:
	
		<add key="MicrosoftAppId" value="88888888-8888-8888-8888-888888888888" />
		<add key="MicrosoftAppPassword" value="aaaa22229999dddd0000999" />

4. In Visual Studio click the play button

5. Once the bot is running, you will need to create a manifest file and sideload it into a team or personal scope to test. All of this can be done in [App Studio for Microsoft Teams](https://docs.microsoft.com/en-us/microsoftteams/platform/get-started/get-started-app-studio).

# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
