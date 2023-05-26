using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Movement : MonoBehaviour
{
	public float leftLimit, rightLimit, frontLimit, backLimit, topLimit, floorLimit;
	public bool lx, rx, fy, ty, fz, bz;
	public int xBal, yBal, zBal;
	public GameObject cam1, cam2;
	public Camera maincam;
	public Vector3 screenPosition, worldPosition;
	Vector3 normal = new Vector3(1.0f, 1.0f, 1.0f);
	Vector3 xcompress = new Vector3(0.5f, 1.0f, 1.0f);
	Vector3 ycompress = new Vector3(1.0f, 0.5f, 1.0f);
	Vector3 zcompress = new Vector3(1.0f, 1.0f, 0.5f);

	void Start()
    {
			cam1.SetActive(true);
			maincam = GameObject.Find("cam1").GetComponent<Camera>();
			cam2.SetActive(false);

			lx = rx = fy = ty = fz = bz = false;

			leftLimit = 455.502f;
			rightLimit = 478.628f;
			frontLimit = 663.7f;
			backLimit = 643.52f;
			topLimit = 16.13f;
			floorLimit = 5.5f;

			xBal = 30;
			yBal = zBal = 20;
    }

		public int smoothcompress(int frames, Vector3 targetscale)
    {
			if(frames > 0){
				transform.localScale = Vector3.Lerp(transform.localScale, targetscale, Time.deltaTime * 20);
				frames--;
			}
			return frames;
    }

		public int smoothexpand(int frames)
		{
			if(frames < 41){
				transform.localScale = Vector3.Lerp(transform.localScale, normal, Time.deltaTime * 41);
				frames++;
			}
			return frames;
		}

    void Update()
    {
			transform.position = worldPosition;
			screenPosition = Input.mousePosition;
			screenPosition.z = maincam.nearClipPlane + 4;

			worldPosition = maincam.ScreenToWorldPoint(screenPosition);

			// Debug.Log(worldPosition.x+ " " + worldPosition.y+ " "+ worldPosition.z);

			if(Input.GetKeyDown(KeyCode.Space))
			{
				if(cam1.activeSelf)
				{
					cam1.SetActive(false);
					cam2.SetActive(true);
					maincam = GameObject.Find("cam2").GetComponent<Camera>();
				}
				else
				{
					cam1.SetActive(true);
					maincam = GameObject.Find("cam1").GetComponent<Camera>();
					cam2.SetActive(false);
				}
			}

			if(Input.GetMouseButtonDown(0))
			{
				SceneManager.LoadScene("SampleScene");
			}

			if(worldPosition.x < leftLimit)
				lx = true;
			if(worldPosition.x > rightLimit)
				rx = true;
			if(worldPosition.y < floorLimit)
				fy = true;
			if(worldPosition.y > topLimit)
				ty = true;
			if(worldPosition.z > frontLimit)
				fz = true;
			if(worldPosition.z < backLimit)
				bz = true;

			if(!fz && !bz && !ty && !fy && !lx && !rx)
			{
				xBal = smoothexpand(xBal);
				yBal = smoothexpand(yBal);
				zBal = smoothexpand(zBal);
			}

			if(rx || lx)
			{
				 worldPosition.x = (rx ? rightLimit : leftLimit);
				 xBal = smoothcompress(xBal, xcompress);
				 rx = lx = false;
			}

			if(fy || ty)
			{
				worldPosition.y = (fy ? floorLimit : topLimit);
				yBal = smoothcompress(yBal, ycompress);
				fy = ty = false;
			}

			if(fz || bz)
			{
				worldPosition.z = (fz ? frontLimit : backLimit);
				zBal = smoothcompress(zBal, zcompress);
				fz = bz = false;
			}
    }
}
