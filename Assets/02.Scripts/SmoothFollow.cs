using UnityEngine;

#pragma warning disable 649
namespace UnityStandardAssets.Utility
{
	public class SmoothFollow : MonoBehaviour
	{
		[SerializeField]
		public Transform target;
		// x-z 평면에서 목표와의 거리
		[SerializeField]
		private float distance = 10.0f;
		// 목표보다 카메라를 올릴 높이
		[SerializeField]
		private float height = 5.0f;

		[SerializeField]
		private float rotationDamping;
		[SerializeField]
		private float heightDamping;

		void LateUpdate()
		{
			if (!target)
				return;

			var currentRotationAngle = Mathf.LerpAngle(transform.eulerAngles.y, target.eulerAngles.y, rotationDamping * Time.deltaTime);
			var currentHeight = Mathf.Lerp(transform.position.y, target.position.y + height, heightDamping * Time.deltaTime);

			// 목표 뒤 distance만큼 x-z 평면에 놓고 높이만 따로 댐핑한다
			var pos = target.position - Quaternion.Euler(0, currentRotationAngle, 0) * Vector3.forward * distance;
			transform.position = new Vector3(pos.x, currentHeight, pos.z);
			transform.LookAt(target);
		}
	}
}
