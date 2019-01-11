using UnityEngine;
using System.Collections;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.DataTypes;
using System.Collections.Generic;
using UnityEngine.UI;

public class speechTT : MonoBehaviour
{
	#region PLEASE SET THESE VARIABLES IN THE INSPECTOR
	[Space(10)]
	[Tooltip("The service URL (optional). This defaults to \"https://stream.watsonplatform.net/speech-to-text/api\"")]
	[SerializeField]
	private string _serviceUrl="https://stream.watsonplatform.net/speech-to-text/api";
	[Tooltip("Text field to display the results of streaming.")]
	public Text ResultsField;
	[Header("CF Authentication")]
	[Tooltip("The authentication username.")]
	[SerializeField]
	private string _username="e27e8d0a-7884-4ee7-85a2-31320a0fc730";
	[Tooltip("The authentication password.")]
	[SerializeField]
	private string _password="zsBaHjrGzpl7";
	[Header("IAM Authentication")]
	[Tooltip("The IAM apikey.")]
	[SerializeField]
	private string _iamApikey="zsBaHjrGzpl7";
	[Tooltip("The IAM url used to authenticate the apikey (optional). This defaults to \"https://iam.bluemix.net/identity/token\".")]
	[SerializeField]
	private string _iamUrl="https://iam.bluemix.net/identity/token";
	#endregion

	private int _recordingRoutine = 0;
	private string _microphoneID = null;
	private AudioClip _recording = null;
	private int _recordingBufferSize = 1;
	private int _recordingHZ = 22050;

	private string msj; //variable mensaje 
	private SpeechToText _service;
    

    private string URLtriste = "https://open.spotify.com/user/spotify/playlist/37i9dQZF1DX7qK8ma5wgG1?si=1h6tjHj5SuqJDDjFqPAX3g";
    private string URLfeliz = "https://open.spotify.com/track/6NPVjNh8Jhru9xOmyQigds?si=apwYIvQ_SM-C0SFMqaVTUA";
    private string URLtranquilo = "https://open.spotify.com/track/6yXc6lrQtoaSX4EeqquXVL?si=zq7kr9A1SRyC6T7UgZMsEQ";
    private string URLestresado = "https://open.spotify.com/track/7JyaABzNBjvXzLvxipcjdI?si=Rh9Zx5wdS_KbzEUOw_gQGg";
    private string URLenojado = "https://open.spotify.com/track/6TPnm7nTohMR2Jds2aSTcV?si=puVLeI4WQ4SK8MTjgCZJdQ";
    private string URLemocionado = "https://open.spotify.com/track/6P47GQTA5q5h9RYm1lCJ6N?si=ZCUXSXHKSQ2gI9nfnH_9mA";
    private string URLcansado = "https://open.spotify.com/track/2fyIS6GXMgUcSv4oejx63f?si=WZsv3ZQnQfqIjF6xzD-aGg";
    private string URLasustado = "https://open.spotify.com/track/2U9IpFtQ5epSfem28Vkj27?si=-i-LxNjvTw-DNrH20HEcAw";
    private string URLaburrido = "https://open.spotify.com/user/jerry21573/playlist/0FAb8PUTP4v3j97S704zuT?si=HFrs--cGREKNUBijnhKTYw";
    private string URLenfiestado = "https://open.spotify.com/track/5Sf3GyLEAzJXxZ5mbCPXTu?si=-oRf8aquS82JyWEVxrVmRA";
    private string URLenamorado = "https://open.spotify.com/track/0j00zu3FKBWSu1Hyt2Ivcj?si=SWUKR-y0SjetfdLHxumm0g";
    private string URLemperrado = "https://open.spotify.com/track/28HOGe3aC0WYqvIGNiMGBt?si=Q9Pr93iOQB6GE68abdTpsQ";

    void Start()
	{
		LogSystem.InstallDefaultReactors();
		Runnable.Run(CreateService());
		msj="";

	}

	private IEnumerator CreateService()
	{
		//  Create credential and instantiate service
		Credentials credentials = null;
		if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password))
		{
			//  Authenticate using username and password
			credentials = new Credentials(_username, _password, _serviceUrl);
		}
		else if (!string.IsNullOrEmpty(_iamApikey))
		{
			//  Authenticate using iamApikey
			TokenOptions tokenOptions = new TokenOptions()
			{
				IamApiKey = _iamApikey,
				IamUrl = _iamUrl
			};

			credentials = new Credentials(tokenOptions, _serviceUrl);

			//  Wait for tokendata
			while (!credentials.HasIamTokenData())
				yield return null;
		}
		else
		{
			throw new WatsonException("Please provide either username and password or IAM apikey to authenticate the service.");
		}

		_service = new SpeechToText(credentials);
		_service.StreamMultipart = true;



		Active = true;
		//StartRecording();
	}

	public bool Active
	{
		get { return _service.IsListening; }
		set
		{
			if (value && !_service.IsListening)
			{
				_service.DetectSilence = true;
				_service.EnableWordConfidence = true;
				_service.EnableTimestamps = true;
				_service.SilenceThreshold = 0.01f;
				_service.MaxAlternatives = 0;
				_service.EnableInterimResults = true;
				_service.OnError = OnError;
				_service.InactivityTimeout = -1;
				_service.ProfanityFilter = false;
				_service.SmartFormatting = true;
				_service.SpeakerLabels = false;
				_service.WordAlternativesThreshold = null;
				_service.StartListening(OnRecognize, OnRecognizeSpeaker);
			}
			else if (!value && _service.IsListening)
			{
				_service.StopListening();
			}
		}
	}

	public void StartRecording()
	{
        Invoke("StopRecording", 5.0f);
		if (_recordingRoutine == 0)
		{
			UnityObjectUtil.StartDestroyQueue();
			_recordingRoutine = Runnable.Run(RecordingHandler());
		}
	}

	public void StopRecording()
	{
        
		if (_recordingRoutine != 0)
		{
			Microphone.End(_microphoneID);
			Runnable.Stop(_recordingRoutine);
			_recordingRoutine = 0;
		}
        reconoceDespliega();
    }

	private void OnError(string error)
	{
		Active = false;

		Log.Debug("ExampleStreaming.OnError()", "Error! {0}", error);
	}

	private IEnumerator RecordingHandler()
	{
		Log.Debug("ExampleStreaming.RecordingHandler()", "devices: {0}", Microphone.devices);
		_recording = Microphone.Start(_microphoneID, true, _recordingBufferSize, _recordingHZ);
		yield return null;      // let _recordingRoutine get set..

		if (_recording == null)
		{
			StopRecording();
			yield break;
		}

		bool bFirstBlock = true;
		int midPoint = _recording.samples / 2;
		float[] samples = null;

		while (_recordingRoutine != 0 && _recording != null)
		{
			int writePos = Microphone.GetPosition(_microphoneID);
			if (writePos > _recording.samples || !Microphone.IsRecording(_microphoneID))
			{
				Log.Error("ExampleStreaming.RecordingHandler()", "Microphone disconnected.");

				StopRecording();
				yield break;
			}

			if ((bFirstBlock && writePos >= midPoint)
				|| (!bFirstBlock && writePos < midPoint))
			{
				// front block is recorded, make a RecordClip and pass it onto our callback.
				samples = new float[midPoint];
				_recording.GetData(samples, bFirstBlock ? 0 : midPoint);

				AudioData record = new AudioData();
				record.MaxLevel = Mathf.Max(Mathf.Abs(Mathf.Min(samples)), Mathf.Max(samples));
				record.Clip = AudioClip.Create("Recording", midPoint, _recording.channels, _recordingHZ, false);
				record.Clip.SetData(samples, 0);

				_service.OnListen(record);

				bFirstBlock = !bFirstBlock;
			}
			else
			{
				// calculate the number of samples remaining until we ready for a block of audio, 
				// and wait that amount of time it will take to record.
				int remaining = bFirstBlock ? (midPoint - writePos) : (_recording.samples - writePos);
				float timeRemaining = (float)remaining / (float)_recordingHZ;

				yield return new WaitForSeconds(timeRemaining);
			}

		}

		yield break;
	}

	private void OnRecognize(SpeechRecognitionEvent result, Dictionary<string, object> customData)
	{
		if (result != null && result.results.Length > 0)
		{
			foreach (var res in result.results)
			{
				foreach (var alt in res.alternatives)
				{
					string text = string.Format("{0} ({1}, {2:0.00})\n", alt.transcript, res.final ? "Final" : "Interim", alt.confidence); //TU mandas lo saldado y es JSON
					Log.Debug("ExampleStreaming.OnRecognize()", text);
					//Log.Debug ("Aqui tenemos el texto que requerimos"+  text);
					msj=alt.transcript; //Asignacion de
					ResultsField.text = text; //
				}

				if (res.keywords_result != null && res.keywords_result.keyword != null)
				{
					foreach (var keyword in res.keywords_result.keyword)
					{
						Log.Debug("ExampleStreaming.OnRecognize()", "keyword: {0}, confidence: {1}, start time: {2}, end time: {3}", keyword.normalized_text, keyword.confidence, keyword.start_time, keyword.end_time);
					}
				}

				if (res.word_alternatives != null)
				{
					foreach (var wordAlternative in res.word_alternatives)
					{
						Log.Debug("ExampleStreaming.OnRecognize()", "Word alternatives found. Start time: {0} | EndTime: {1}", wordAlternative.start_time, wordAlternative.end_time);
						foreach(var alternative in wordAlternative.alternatives)
							Log.Debug("ExampleStreaming.OnRecognize()", "\t word: {0} | confidence: {1}", alternative.word, alternative.confidence);
					}
				}
			}
			//reconoceDespliega ();
		}

	}
    

	public void reconoceDespliega(){
		Debug.Log ("Mensaje: "+msj); //imrpimimos el mensaje
		if(msj.Contains("triste") || msj.Contains("decaido") || msj.Contains("desanimado"))
        {
            openSpotify(0);
        }
		if(msj.Contains("feliz") || msj.Contains("contento") || msj.Contains("alegre"))
        {

            openSpotify(1);
        }
		if(msj.Contains("tranquilo") || msj.Contains("calmado") || msj.Contains("pacifico") || msj.Contains("relajado") || msj.Contains("relax"))
        {

            openSpotify(2);
        }
		if(msj.Contains("estresado") || msj.Contains("inquieto"))
        {

            openSpotify(3);
        }
		if(msj.Contains("enojado") || msj.Contains("molesto") || msj.Contains("furioso") || msj.Contains("emperrado") || msj.Contains("encabronado"))
        {

            openSpotify(4);
        }
		if(msj.Contains("emocionado") || msj.Contains("entusiasmado") || msj.Contains("ansioso"))
        {

            openSpotify(5);
        }
		if(msj.Contains("cansado") || msj.Contains("agotado") || msj.Contains("tired"))
        {

            openSpotify(6);
        }
		if(msj.Contains("asustado") || msj.Contains("nervioso") || msj.Contains("scared"))
        {

            openSpotify(7);
        }
		if(msj.Contains("aburrido") || msj.Contains("bored"))
        {

            openSpotify(8);
        }
		if(msj.Contains("enfiestado")){

            openSpotify(9);
        }
        if (msj.Contains("enamorado") || msj.Contains("caliente") || msj.Contains("excitado") || msj.Contains("horny"))
        {

            openSpotify(10);
        }
        
    }

    private void openSpotify(int id)
    {
        switch (id)
        {
            case 0:
                Application.OpenURL(URLtriste);
                break;
            case 1:
                Application.OpenURL(URLfeliz);
                break;
            case 2:
                Application.OpenURL(URLtranquilo);
                break;
            case 3:
                Application.OpenURL(URLestresado);
                break;
            case 4:
                Application.OpenURL(URLenojado);
                break;
            case 5:
                Application.OpenURL(URLemocionado);
                break;
            case 6:
                Application.OpenURL(URLcansado);
                break;
            case 7:
                Application.OpenURL(URLasustado);
                break;
            case 8:
                Application.OpenURL(URLaburrido);
                break;
            case 9:
                Application.OpenURL(URLenfiestado);
                break;
            case 10:
                Application.OpenURL(URLenamorado);
                break;

        }
    }

	private void OnRecognizeSpeaker(SpeakerRecognitionEvent result, Dictionary<string, object> customData)
	{
		if (result != null)
		{
			foreach (SpeakerLabelsResult labelResult in result.speaker_labels)
			{
				Log.Debug("ExampleStreaming.OnRecognize()", string.Format("speaker result: {0} | confidence: {3} | from: {1} | to: {2}", labelResult.speaker, labelResult.from, labelResult.to, labelResult.confidence));
			}
		}
	}
}
