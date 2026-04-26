using Sirenix.OdinInspector;
using UnityEngine;
using Utils.RendererEffect;

public class CustomRE : _RendererEffect
{
	private MaterialPropertyBlock m_propertyBlock;

	private void Awake()
	{
		m_propertyBlock = new MaterialPropertyBlock();
	}

	private void EnsurePropertyBlock()
	{
		if (m_propertyBlock == null)
			m_propertyBlock = new MaterialPropertyBlock();
	}

	[Button]
	public void ChangeFloat(string _property, float value)
	{
		EnsurePropertyBlock();

		if (string.IsNullOrWhiteSpace(_property))
			return;

		foreach (var r in renderers)
		{
			if (r == null) continue;

			r.GetPropertyBlock(m_propertyBlock);
			m_propertyBlock.SetFloat(_property, value);
			r.SetPropertyBlock(m_propertyBlock);
		}
	}

	[Button]
	public void ChangeInt(string _property, int value)
	{
		EnsurePropertyBlock();

		if (string.IsNullOrWhiteSpace(_property))
			return;

		foreach (var r in renderers)
		{
			if (r == null) continue;

			r.GetPropertyBlock(m_propertyBlock);
			m_propertyBlock.SetInt(_property, value);
			r.SetPropertyBlock(m_propertyBlock);
		}
	}

	[Button]
	public void ChangeColor(string _property, Color value)
	{
		EnsurePropertyBlock();

		if (string.IsNullOrWhiteSpace(_property))
			return;

		foreach (var r in renderers)
		{
			if (r == null) continue;

			r.GetPropertyBlock(m_propertyBlock);
			m_propertyBlock.SetColor(_property, value);
			r.SetPropertyBlock(m_propertyBlock);
		}
	}

	[Button]
	public void ChangeVector(string _property, Vector4 value)
	{
		EnsurePropertyBlock();

		if (string.IsNullOrWhiteSpace(_property))
			return;

		foreach (var r in renderers)
		{
			if (r == null) continue;

			r.GetPropertyBlock(m_propertyBlock);
			m_propertyBlock.SetVector(_property, value);
			r.SetPropertyBlock(m_propertyBlock);
		}
	}

	[Button]
	public void ClearOverrides()
	{
		foreach (var r in renderers)
		{
			if (r == null) continue;
			r.SetPropertyBlock(null);
		}
	}
}