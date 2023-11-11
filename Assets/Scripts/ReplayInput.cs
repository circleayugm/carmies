/*
 *	ReplayInput.cs
 *		���v���C���͗p�N���X�ϐ�
 * 
 * 
 * 
 * 
 *	20221211	3���O���炢�ɍ쐬
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayInput
{
	public uint random_seed{ get; set; }
	public uint player{ get; set; }
	public Vector2 vector{ get; set; }
	public bool button{ get; set; }
	public int count{ get; set; }

	public ReplayInput()
	{
		random_seed = 0;            // �����_���V�[�h
		player = 1;					// �v���C���[�ԍ�(�R���g���[���[�擾�E�p�P�b�g�T���p)
		vector = new Vector2(0, 0);	// �X�e�B�b�N�̌X��
		button = false;				// �{�^��
		count = -1;					// �i�s�J�E���^
	}
}
