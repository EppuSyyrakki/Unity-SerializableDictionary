﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
	[SerializeField]
	TKey[] m_keys;
	[SerializeField]
	TValue[] m_values;

    public void OnAfterDeserialize()
    {
		if(m_keys != null && m_values != null && m_keys.Length == m_values.Length)
		{
	        base.Clear();
			int n = m_keys.Length;
			for(int i = 0; i < n; ++i)
			{
				base[m_keys[i]] = m_values[i];
			}

			m_keys = null;
			m_values = null;
		}

    }

    public void OnBeforeSerialize()
    {
        int n = base.Count;
		m_keys = new TKey[n];
		m_values = new TValue[n];

		int i = 0;
		foreach(var kvp in this)
		{
			m_keys[i] = kvp.Key;
			m_values[i] = kvp.Value;
			++i;
		}
    }
}