/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@avatarsdk.com>, January 2025
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AvatarSDK.Leap
{
	public enum AvatarGenerationStatusCode
	{
		Success = 0,
		
		LogicError = -1,
		RuntimeError = -2,
		OtherError = -3,
		UnknownError = -4,
		NoAVXError = -5,
		StatsFileError = -6,
		NoParamsError = -7,
		LicenseFileNotFoundError = -8,
		InvalidLicenseError = -9,
		LicenseError = -10,

		ArchiveExtractionError = -100,

		LicenseRequestMinError = 1000,
		LicenseRequestMaxError = 1999,

		LicenseAuthMinError = 2000,
		LicenseAuthMaxError = 2999,

		ExportMinError = 3000,
	}

	public static class AvatarGenerationStatusCodeExtenion
	{ 
		public static string Message(this AvatarGenerationStatusCode statusCode)
		{
			switch (statusCode)
			{
				case AvatarGenerationStatusCode.Success:
					return "Avatar successfully generated.";
				case AvatarGenerationStatusCode.NoAVXError:
					return "Unsupported hardware.";
				case AvatarGenerationStatusCode.NoParamsError:
					return "Inavlid arguments.";
				case AvatarGenerationStatusCode.LicenseFileNotFoundError:
					return "License file not found.";
				case AvatarGenerationStatusCode.InvalidLicenseError:
					return "Invalid license.";
				case AvatarGenerationStatusCode.LicenseError:
					return "Unable to read license file.";
				case AvatarGenerationStatusCode.ArchiveExtractionError:
					return "Error occured during archive extraction.";
			}

			if (statusCode >= AvatarGenerationStatusCode.LicenseRequestMinError && statusCode <= AvatarGenerationStatusCode.LicenseRequestMaxError)
				return "Error occured during license verification request.";

			if (statusCode >= AvatarGenerationStatusCode.LicenseAuthMinError && statusCode <= AvatarGenerationStatusCode.LicenseAuthMaxError)
				return "Error occured during authentication request.";

			if (statusCode > AvatarGenerationStatusCode.ExportMinError)
				return "Error during avatar export.";

			return "Error occured.";
		}
	}
}
