using System.IO;
using UnityEngine;

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
			// TODO: Implement
			string installScriptPath = Path.Combine(Application.streamingAssetsPath, "unitycapture", "Install.sh");
			//System.Diagnostics.Process.Start(Path.Combine(Application.streamingAssetsPath, "v4l2loopback", "Install.sh"));
			ExecuteShellCommand("sh", installScriptPath);


		}

		public static void UninstallVirtualCamera() {
			// TODO: Implement
			//System.Diagnostics.Process.Start(Path.Combine(Application.streamingAssetsPath, "v4l2loopback", "Uninstall.sh"));
			string uninstallScriptPath = Path.Combine(Application.streamingAssetsPath, "unitycapture", "Uninstall.sh");

			// Verificar que el script de desinstalación exista
			if (File.Exists(uninstallScriptPath))
			{
				// Ejecutar el script de desinstalación a través de la Terminal
				ExecuteShellCommand("sh", uninstallScriptPath);
			}
			else
			{
				Debug.LogError("El script de desinstalación no se encuentra en la ruta especificada.");
			}

		}

		private static void ExecuteShellCommand(string command, string arguments)
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
					Debug.LogError("Error executing shell command: " + error);
				}

				Debug.Log("Shell command output: " + output);
			}
		}
#endif
	}
}
