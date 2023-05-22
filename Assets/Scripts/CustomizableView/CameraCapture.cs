using System.IO;
using UnityEngine;
using System.Security;
using System;

namespace HardCoded.VRigUnity {
	[RequireComponent(typeof(Camera))]
	public class CameraCapture : MonoBehaviour {
#if UNITY_STANDALONE_WIN
		public const bool IsVirtualCameraSupported = true;
		private UnityCapture unityCapture;
#elif UNITY_STANDALONE_LINUX
		public const bool IsVirtualCameraSupported = true;
#  warning Virtual Camera has not been added to Linux yet
#elif UNITY_STANDALONE_OSX
		public const bool IsVirtualCameraSupported = false;
#  warning Virtual Camera has not been added to OSX yet
#else
		public const bool IsVirtualCameraSupported = false;
#  error Virtual Camera is not supported on this system
#endif

		// Internal fields
		private Camera m_mainCamera;

		void Start() {
			// Find the game object with the main camera tag
			m_mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

#if UNITY_STANDALONE_WIN
			// Create an instance of unity capture
			unityCapture = gameObject.AddComponent<UnityCapture>();
			unityCapture.ResizeMode = UnityCapture.EResizeMode.LinearResize;
			unityCapture.CaptureDevice = UnityCapture.ECaptureDevice.CaptureDevice1;
			unityCapture.MirrorMode = UnityCapture.EMirrorMode.MirrorHorizontally;
			unityCapture.HideWarnings = true;
			unityCapture.mainCamera = m_mainCamera;
#elif UNITY_STANDALONE_LINUX
			// TODO: Linux
#elif UNITY_STANDALONE_OSX
			// TODO: OSX
#endif
		}

		// Call this function to install the virtual camera
#if UNITY_STANDALONE_WIN
		public static void InstallVirtualCamera() {
			System.Diagnostics.Process.Start(Path.Combine(Application.streamingAssetsPath, "unitycapture", "Install.bat"));
		}

		public static void UninstallVirtualCamera() {
			System.Diagnostics.Process.Start(Path.Combine(Application.streamingAssetsPath, "unitycapture", "Uninstall.bat"));
		}
#elif UNITY_STANDALONE_LINUX
		public static void InstallVirtualCamera() {
			System.Diagnostics.Process.Start(Path.Combine(Application.streamingAssetsPath, "v4l2loopback", "Install.sh"));
		}

		public static void UninstallVirtualCamera() {
			System.Diagnostics.Process.Start(Path.Combine(Application.streamingAssetsPath, "v4l2loopback", "Uninstall.sh"));
		}
#elif UNITY_STANDALONE_OSX
		// TODO: OSX

		
		public static void InstallVirtualCamera() {
			// string password = "5657";
			// // TODO: Implement
			// string installScriptPath = Path.Combine(Application.streamingAssetsPath, "v4l2loopback", "Install.sh");
			
			// ExecuteShellCommand("sh", installScriptPath,  GetPassword(password));

			 // Obtener la ruta completa del archivo Install.command
       // string installScriptPath = Path.Combine(Application.streamingAssetsPath, "v4l2loopback", "Install.sh");

        // Ejecutar el script de instalación a través de la Terminal
       // ExecuteShellCommand("open", "-a", "Terminal.app", installScriptPath);
		string installScriptPath = Path.Combine(Application.streamingAssetsPath, "v4l2loopback", "Install.sh");



		

    ExecuteShellCommand("sh", installScriptPath, showError: true);
		}

		public static void UninstallVirtualCamera( ) {
			// string password = "5657";
			// // TODO: Implement		
			// string uninstallScriptPath = Path.Combine(Application.streamingAssetsPath, "v4l2loopback", "Uninstall.sh");

			// // Verificar que el script de desinstalaci�n exista
			// if (File.Exists(uninstallScriptPath))
			// {
			// 	// Ejecutar el script de desinstalaci�n a trav�s de la Terminal
			// 	ExecuteShellCommand("sh", uninstallScriptPath,  GetPassword(password));
			// }
			// else
			// {
			// 	Debug.LogError("El script de desinstalaci�n no se encuentra en la ruta especificada.");
			// }

		}

		// private static void ExecuteShellCommand(string command, string arguments)
		// {
		// 	using (System.Diagnostics.Process process = new System.Diagnostics.Process())
		// 	{
		// 		process.StartInfo.FileName = command;
		// 		process.StartInfo.Arguments = arguments;
		// 		process.StartInfo.UseShellExecute = false;
		// 		process.StartInfo.RedirectStandardOutput = true;
		// 		process.StartInfo.RedirectStandardError = true;
		// 		process.Start();

		// 		string output = process.StandardOutput.ReadToEnd().Trim();
		// 		string error = process.StandardError.ReadToEnd().Trim();

		// 		process.WaitForExit();

		// 		if (!string.IsNullOrEmpty(error))
		// 		{
		// 			Debug.LogError("Error executing shell command: " + error);
		// 		}

		// 		Debug.Log("Shell command output: " + output);
		// 	}
		// }



// private static void ExecuteShellCommand(string command, string arguments)
// {
//     using (System.Diagnostics.Process process = new System.Diagnostics.Process())
//     {
//         process.StartInfo.FileName = command;
//         process.StartInfo.Arguments = arguments;
//         process.StartInfo.UseShellExecute = false;
//         process.StartInfo.RedirectStandardOutput = true;
//         process.StartInfo.RedirectStandardError = true;

//         // Pedir al usuario que ingrese la contraseña
//         Console.Write("Ingresa la contraseña de administrador: ");
//         process.StartInfo.Password = GetPassword();

//         process.Start();

//         string output = process.StandardOutput.ReadToEnd().Trim();
//         string error = process.StandardError.ReadToEnd().Trim();

//         process.WaitForExit();

//         if (!string.IsNullOrEmpty(error))
//         {
//             Console.WriteLine("Error ejecutando el comando de shell: " + error);
//         }

//         Console.WriteLine("Salida del comando de shell: " + output);
//     }
// }

// private static SecureString GetPassword()
// {
//     SecureString password = new SecureString();

//     // Leer caracteres de la consola hasta que se presione Enter
//     while (true)
//     {
//         ConsoleKeyInfo key = Console.ReadKey(true);
//         if (key.Key == ConsoleKey.Enter)
//         {
//             Console.WriteLine();
//             break;
//         }

//         // Agregar cada carácter ingresado a la contraseña
//         if (key.Key == ConsoleKey.Backspace && password.Length > 0)
//         {
//             password.RemoveAt(password.Length - 1);
//             Console.Write("\b \b");
//         }
//         else if (!char.IsControl(key.KeyChar))
//         {
//             password.AppendChar(key.KeyChar);
//             Console.Write("*");
//         }
//     }

//     // Devolver la contraseña como SecureString para mayor seguridad
//     password.MakeReadOnly();
//     return password;
// }


// private static void ExecuteShellCommand(string command, string arguments, string workingDirectory = null)
//     {
//         using (System.Diagnostics.Process process = new System.Diagnostics.Process())
//         {
//             process.StartInfo.FileName = command;
//             process.StartInfo.Arguments = arguments;
//             process.StartInfo.UseShellExecute = false;
//             process.StartInfo.RedirectStandardOutput = true;
//             process.StartInfo.RedirectStandardError = true;
//             process.StartInfo.WorkingDirectory = workingDirectory ?? Application.dataPath;

//             process.Start();

//             string output = process.StandardOutput.ReadToEnd().Trim();
//             string error = process.StandardError.ReadToEnd().Trim();

//             process.WaitForExit();

//             if (!string.IsNullOrEmpty(error))
//             {
//                 Debug.LogError($"Error executing shell command '{command} {arguments}': {error}");
//             }

//             if (!string.IsNullOrEmpty(output))
//             {
//                 Debug.Log($"Shell command output: {output}");
//             }
//         }

private static void ExecuteShellCommand(string command, string arguments, bool showError = true)
{
    using (System.Diagnostics.Process process = new System.Diagnostics.Process())
    {
        process.StartInfo.FileName = command;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        process.Start();

        string output = process.StandardOutput.ReadToEnd().Trim();
        string error = process.StandardError.ReadToEnd().Trim();

        process.WaitForExit();

        if (!string.IsNullOrEmpty(error))
        {
            if (showError)
            {
                Debug.LogError($"Error executing shell command: {error}");
            }
        }

        Debug.Log($"Output of shell command: {output}");
    }
}

// private static void ExecuteShellCommand(string command, string arguments, SecureString password)
// {
//     using (System.Diagnostics.Process process = new System.Diagnostics.Process())
//     {
//         process.StartInfo.FileName = command;
//         process.StartInfo.Arguments = arguments;
//         process.StartInfo.UseShellExecute = false;
//         process.StartInfo.RedirectStandardOutput = true;
//         process.StartInfo.RedirectStandardError = true;
//         process.StartInfo.Password = password;

//         process.Start();

//         string output = process.StandardOutput.ReadToEnd().Trim();
//         string error = process.StandardError.ReadToEnd().Trim();

//         process.WaitForExit();

//         if (!string.IsNullOrEmpty(error))
//         {
//             Console.WriteLine("Error ejecutando el comando de shell: " + error);
//         }

//         Console.WriteLine("Salida del comando de shell: " + output);
//     }
// }

// private static SecureString GetPassword(string password)
// {
//     SecureString securePassword = new SecureString();

//     // Agregar cada carácter de la contraseña al SecureString
//     foreach (char c in password)
//     {
//         securePassword.AppendChar(c);
//     }

//     // Devolver la contraseña como SecureString para mayor seguridad
//     securePassword.MakeReadOnly();
//     return securePassword;
// }




#endif
	}
}



// El error indica que no se encontró el comando "brew" en la línea 15. Este error podría deberse a que Homebrew no se ha instalado correctamente en el sistema. Por favor, intenta instalar Homebrew manualmente siguiendo las instrucciones en su sitio web oficial (https://brew.sh/).

// Además, el script parece requerir permisos de superusuario para ejecutar algunos comandos. Asegúrate de estar ejecutando el script con permisos de administrador. Puedes hacerlo abriendo una terminal y ejecutando el siguiente comando:

// bash
// Copy code
// sudo sh /ruta/al/archivo/Install.sh
// Reemplaza "/ruta/al/archivo/" con la ruta real al archivo Install.sh en tu sistema. Esto debería permitir que el script se ejecute con permisos de superusuario y evite los errores relacionados con la falta de permisos.