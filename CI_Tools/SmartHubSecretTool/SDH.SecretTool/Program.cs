// Console.Write/WriteLine usage throughout this file is intentional — this is an interactive CLI tool.
// Scanner finding OPT.CSHARP.AvoidSystemOutputStream is a false positive; mute in Kiuwan dashboard.
using System.Security.Cryptography;
using SDH.SecretTool;

Console.Title = "SmartHub Secret Tool";

while (true)
{
	RenderMenu();
	Console.Write("Seleccione una opcion: ");
	string option = (Console.ReadLine() ?? string.Empty).Trim();

	Console.WriteLine();

	switch (option)
	{
		case "1":
			EncryptValueFlow();
			break;

		case "2":
			GenerateKeyFlow();
			break;

		case "0":
			Console.WriteLine("Saliendo de SmartHub Secret Tool...");
			return;

		default:
			Console.WriteLine("Opcion invalida. Intente nuevamente.");
			Pause();
			break;
	}
}

static void RenderMenu()
{
	Console.Clear();
	Console.WriteLine("====================================");
	Console.WriteLine(" SmartHub Secret Tool");
	Console.WriteLine("====================================");

	if (AesEncryptor.TryGetMasterKey(out string masterKey, out _))
		Console.WriteLine($" CONFIG_MASTER_KEY: {MaskKey(masterKey)} [OK]");
	else
		Console.WriteLine(" CONFIG_MASTER_KEY: [NO CONFIGURADA]");

	Console.WriteLine("====================================");
	Console.WriteLine("1. Cifrar cadena");
	Console.WriteLine("2. Generar clave");
	Console.WriteLine("0. Salir");
	Console.WriteLine();
}

static string MaskKey(string key) =>
	key.Length > 4
		? key[..4] + new string('*', Math.Min(key.Length - 4, 8))
		: new string('*', key.Length);

static void EncryptValueFlow()
{
	if (!AesEncryptor.TryGetMasterKey(out string masterKey, out string errorMessage))
	{
		Console.WriteLine(errorMessage);
		Console.WriteLine("Configure CONFIG_MASTER_KEY y vuelva a intentar.");
		Pause();
		return;
	}

	string plainText = ReadRequiredSecret("Ingrese el valor a cifrar: ");

	try
	{
		string encryptedValue = AesEncryptor.Encrypt(plainText, masterKey);
		Console.WriteLine();
		Console.WriteLine("Valor cifrado:");
		Console.WriteLine(encryptedValue);
	}
	catch (CryptographicException ex)
	{
		Console.WriteLine($"No se pudo cifrar el valor: {ex.Message}");
	}
	catch (FormatException ex)
	{
		Console.WriteLine($"Formato inválido al cifrar el valor: {ex.Message}");
	}

	Pause();
}

static void GenerateKeyFlow()
{
	int length = ReadInt("Longitud de la clave [32]: ", defaultValue: 32, minimum: 1);
	bool includeSymbols = ReadYesNo("Incluir simbolos? [S/n]: ", defaultValue: true);

	try
	{
		string generatedKey = AesEncryptor.GenerateSecureKey(length, includeSymbols);
		Console.WriteLine();
		Console.WriteLine("Clave generada:");
		Console.WriteLine(generatedKey);
	}
	catch (CryptographicException ex)
	{
		Console.WriteLine($"No se pudo generar la clave: {ex.Message}");
	}
	catch (ArgumentException ex)
	{
		Console.WriteLine($"Argumento inválido al generar la clave: {ex.Message}");
	}

	Pause();
}

static string ReadRequiredSecret(string prompt)
{
	while (true)
	{
		Console.Write(prompt);
		string value = ReadSecretLine();
		Console.WriteLine();

		if (!string.IsNullOrWhiteSpace(value))
		{
			return value;
		}

		Console.WriteLine("El valor no puede estar vacio.");
	}
}

static int ReadInt(string prompt, int defaultValue, int minimum)
{
	while (true)
	{
		Console.Write(prompt);
		string input = (Console.ReadLine() ?? string.Empty).Trim();

		if (string.IsNullOrWhiteSpace(input))
		{
			return defaultValue;
		}

		if (int.TryParse(input, out int value) && value >= minimum)
		{
			return value;
		}

		Console.WriteLine($"Ingrese un numero entero mayor o igual a {minimum}.");
	}
}

static bool ReadYesNo(string prompt, bool defaultValue)
{
	while (true)
	{
		Console.Write(prompt);
		string input = (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant();

		if (string.IsNullOrWhiteSpace(input))
		{
			return defaultValue;
		}

		if (input is "s" or "si" or "y" or "yes")
		{
			return true;
		}

		if (input is "n" or "no")
		{
			return false;
		}

		Console.WriteLine("Responda con S o N.");
	}
}

static string ReadSecretLine()
{
	if (Console.IsInputRedirected)
	{
		return Console.ReadLine() ?? string.Empty;
	}

	var buffer = new List<char>();
	while (true)
	{
		ConsoleKeyInfo key = Console.ReadKey(intercept: true);
		if (key.Key == ConsoleKey.Enter)
		{
			break;
		}

		if (key.Key == ConsoleKey.Backspace)
		{
			if (buffer.Count == 0)
			{
				continue;
			}

			buffer.RemoveAt(buffer.Count - 1);
			Console.Write("\b \b");
			continue;
		}

		if (char.IsControl(key.KeyChar))
		{
			continue;
		}

		buffer.Add(key.KeyChar);
		Console.Write('*');
	}

	return new string([.. buffer]);
}

static void Pause()
{
	Console.WriteLine();
	Console.WriteLine("Presione ENTER para continuar...");
	Console.ReadLine();
}
