using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace KeyVault
{
    class Program
    {
    	//TODO ClientId from Azure AD goes here
        static string clientId = "MyClientId";
	    //TODO Client secret from Azure AD goes here
        static string clientSecret = "MyClientSecret";
	    //TODO Url for Azure KeyVault goes here
	    static string vaultAddress = "MyVaultUrl";

        static async Task Main(string[] args)
        {
            KeyVaultClient vaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(Program.GetToken));

            string option;
            do
            {
                WriteMenu();

                option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        await ListSecrets(vaultClient, vaultAddress);
                        break;
                    case "2":
                        await GetSecretValue(vaultClient, vaultAddress);
                        break;
                    case "3":
                        await SetSecretValue(vaultClient, vaultAddress);
                        break;
                    case "4":
                        await DeleteSecret(vaultClient, vaultAddress);
                        break;
                    case "9":
                        break;
                    default: 
                        WriteMenu();
                        break;
                }
            } while (option != "9");
        }

        private static async Task DeleteSecret(KeyVaultClient vaultClient, string vaultAddress)
        {
            Console.WriteLine("Enter the id of the secret to be deleted");
            string secretName = Console.ReadLine();
            await vaultClient.DeleteSecretAsync(vaultAddress, secretName);
        }

        private static async Task SetSecretValue(KeyVaultClient vaultClient, string vaultAddress)
        {
            Console.WriteLine("Enter the id of the secret you want to set");
            string secretName = Console.ReadLine();

            Console.WriteLine($"Enter the new value for {secretName}");

            StringBuilder secretBuilder = new StringBuilder();
            bool continueReading = true;
            char newLineChar = '\r';
            while (continueReading)
            {
                // ReadKey has an overload that intercepts keystrokes and stops them being visible onscreen
                ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(true);
                char secretChar = consoleKeyInfo.KeyChar;

                if (secretChar == newLineChar)
                {
                    continueReading = false;
                }
                else
                {
                    secretBuilder.Append(secretChar.ToString());
                }
            }

            var secretValue = secretBuilder.ToString();

            await vaultClient.SetSecretAsync(vaultAddress, secretName, secretValue);
        }

        private static async Task GetSecretValue(KeyVaultClient vaultClient, string vaultAddress)
        {
            Console.WriteLine("Enter the secret Id");
            string secretName = Console.ReadLine();

            var secret = await vaultClient.GetSecretAsync(vaultAddress, secretName);

            Console.WriteLine($"Current value of {secretName}: {secret.Value}");

            Console.WriteLine("Press Enter to continue");
            Console.ReadLine();
        }

        private static async Task ListSecrets(KeyVaultClient client, string vaultAddress)
        {
            var secrets = await client.GetSecretsAsync(vaultAddress);
            foreach (SecretItem secretItem in secrets)
            {
                Console.WriteLine(secretItem.Id);
            }

            Console.WriteLine("Press Enter to continue");
            Console.ReadLine();
        }

        private static void WriteMenu()
        {
            Console.Clear();
            Console.WriteLine("--- Azure KeyVault Console Demo ---");
            Console.WriteLine("1. List secret Ids");
            Console.WriteLine("2. Get a secret value");
            Console.WriteLine("3. Set a secret value");
            Console.WriteLine("4. Delete a secret");
            Console.WriteLine("9. Exit");
            Console.WriteLine("Enter a number to make a selection:");
        }

        private static async Task<string> GetToken(string authority, string resource, string scope)
        {
            ClientCredential credential = new ClientCredential(clientId, clientSecret);

            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);

            var result = await context.AcquireTokenAsync(resource, credential);

            return result.AccessToken;
        }
    }
}
