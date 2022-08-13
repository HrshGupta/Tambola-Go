using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZXing;
using ZXing.QrCode;


public class QRReader : MonoBehaviour
{
    [SerializeField] UnityEngine.UI.Text linkURLText;
    [SerializeField] UnityEngine.UI.Button openLink;
    string linkURL;
    [SerializeField] WebView webView;
    [SerializeField] GameObject webpageHolder;
    public void GetQRFromGallery()
    {
		NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
		{
            Texture2D texture = NativeGallery.LoadImageAtPath(path);  //this is non readable and will not be decode, Hence create a copy
            Texture2D duplicate = duplicateTexture(texture);

            if (texture == null)
            {
                Debug.Log("Couldn't load texture from " + path);
                return;
            }

            BarcodeReader barCodeReader = new BarcodeReader();
            var data = barCodeReader.Decode(duplicate.GetPixels32() , duplicate.width , duplicate.height);
            Debug.Log(data);
            linkURLText.text = data.ToString();
            linkURL = data.ToString();
            webView.Url = linkURL;
        });
	}

    public void OpenURL()
    {
        //webpageHolder.SetActive(true);
        Application.OpenURL(linkURL);
        //webView.OpenURL();
    }

    Texture2D duplicateTexture(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }
}
