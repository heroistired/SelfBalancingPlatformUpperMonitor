						  /*********************************************************************************************
�������� ���� 10λADCת��ʵ�����
��д�ˣ� ���� ����
��дʱ�䣺����2010��3��24��
Ӳ��֧�֣�����STC12C5A60S2 ʹ��10λADC���� �ⲿ����12MHz
�ӿ�˵��������P1.0�ӿڽӵ�λ��  
�޸���־������
����1-								
/*********************************************************************************************
˵����
PC���ڶ����� [ 4800��8���ޣ�1���� ]
��ADC��������ֵͨ��������ʮ�����Ʒ�ʽ��ʾ����һ��������ADC��8λ���ڶ���������ADC��2λ����
/*********************************************************************************************/

#include <STC12C5A60S2.H> //��Ƭ��ͷ�ļ�
#include <intrins.h>	//51�������㣨����_nop_�պ�����

/*********************************************************************************************
�����������뼶CPU��ʱ����
��  �ã�DELAY_MS (?);
��  ����1~65535����������Ϊ0��
����ֵ����
��  ����ռ��CPU��ʽ��ʱ�������ֵ��ͬ�ĺ���ʱ��
��  ע��Ӧ����1T��Ƭ��ʱi<600��Ӧ����12T��Ƭ��ʱi<125
/*********************************************************************************************/
void DELAY_MS (unsigned int a){
	unsigned int i;
	while( a-- != 0){
		for(i = 0; i < 600; i++);
	}
}
/*********************************************************************************************/

/*********************************************************************************************
��������UART���ڳ�ʼ������
��  �ã�UART_init();
��  ������
����ֵ����
��  ��������UART���ڽ����жϣ��������ڽ��գ�����T/C1���������ʣ�ռ�ã�
��  ע���񵴾���Ϊ12MHz��PC���ڶ����� [ 4800��8���ޣ�1���� ]
/**********************************************************************************************/
void UART_init (void){
	//EA = 1; //�������жϣ��粻ʹ���жϣ�����//���Σ�
	//ES = 1; //����UART���ڵ��ж�

	TMOD = 0x20;	//��ʱ��T/C1������ʽ2
	SCON = 0x50;	//���ڹ�����ʽ1���������ڽ��գ�SCON = 0x40 ʱ��ֹ���ڽ��գ�
	TH1 = 0xF3;	//��ʱ����ֵ��8λ����
	TL1 = 0xF3;	//��ʱ����ֵ��8λ����
	PCON = 0x80;	//�����ʱ�Ƶ�����α��䲨����Ϊ2400��
	TR1 = 1;	//��ʱ������    
}
/**********************************************************************************************/

/*********************************************************************************************
��������UART���ڷ��ͺ���
��  �ã�UART_T (?);
��  ������ҪUART���ڷ��͵����ݣ�8λ/1�ֽڣ�
����ֵ���� 
��  �����������е����ݷ��͸�UART���ڣ�ȷ�Ϸ�����ɺ��˳�
��  ע��
/**********************************************************************************************/
void UART_T (unsigned char UART_data){ //���崮�ڷ������ݱ���
	SBUF = UART_data;	//�����յ����ݷ��ͻ�ȥ
	while(TI == 0);		//��鷢���жϱ�־λ
	TI = 0;			//����жϱ�־λΪ0���������㣩
}
/**********************************************************************************************/
/*********************************************************************************************
��������10λA/Dת����ʼ������
��  �ã�Read_init (?);
��  ��������Ķ˿ڣ�0000 0XXX ����XXX����������˿ںţ�����ʮ����0~7��ʾ��0��ʾP1.0��7��ʾP1.7��
����ֵ����
��  ��������ADC���ܲ�����ADC������˿�
��  ע��������STC12C5A60S2ϵ�е�Ƭ��������ʹ��STC12C5A60S2.hͷ�ļ���
/**********************************************************************************************/
void Read_init (unsigned char CHA){
	unsigned char AD_FIN=0; //�洢A/Dת����־
    CHA &= 0x07;            //ѡ��ADC��8���ӿ��е�һ����0000 0111 ��0��5λ��
    ADC_CONTR = 0x40;		//ADCת�����ٶȣ�0XX0 0000 ����XX�����ٶȣ�����������ֲ����ã�
    _nop_();
    ADC_CONTR |= CHA;       //ѡ��A/D��ǰͨ��
    _nop_();
    ADC_CONTR |= 0x80;      //����A/D��Դ
    DELAY_MS(1);            //ʹ�����ѹ�ﵽ�ȶ���1ms���ɣ�
}
/**********************************************************************************************/
/*********************************************************************************************
��������10λA/Dת������
��  �ã�ADC_Read ();
��  ������
����ֵ���ޣ�10λADC���ݸ�8λ�����ADC_RES�У���2λ�����ADC_RESL�У�
��  ��������ָ��ADC�ӿڵ�A/Dת��ֵ����������ֵ
��  ע��������STC12C5A60S2ϵ�е�Ƭ��������ʹ��STC12C5A60S2.hͷ�ļ���
/**********************************************************************************************/
void ADC_Read (void){
	unsigned char AD_FIN=0; //�洢A/Dת����־
    ADC_CONTR |= 0x08;      //����A/Dת����0000 1000 ��ADCS = 1��
    _nop_();
    _nop_();
    _nop_();
    _nop_();
    while (AD_FIN ==0){     //�ȴ�A/Dת������
        AD_FIN = (ADC_CONTR & 0x10); //0001 0000����A/Dת��������
    }
    ADC_CONTR &= 0xE7;      //1111 0111 ��ADC_FLAGλ, �ر�A/Dת��, 
}
/**********************************************************************************************/
/*********************************************************************************************
��������������
��  �ã���
��  ������
����ֵ����
��  ��������ʼ��������ѭ��
��  ע��
/**********************************************************************************************/
void main (void){
	UART_init();//���ڳ�ʼ����
	Read_init(0);//ADC��ʼ��
	P1M1 = 0x01; //P1.7~P1.0��0000 0001�����裩//ע�⣺����ADCͨ��ʱ��ͬʱ����Ӧ��IO�ӿ��޸�Ϊ�������롣
	P1M0 = 0x00; //P1.7~P1.0��0000 0000��ǿ�ƣ�
	while(1){
	    ADC_Read ();//����ADCת������
	    UART_T (((ADC_RES << 2 ) + ADC_RESL)&0xff); //����С���飬��ADC����ֵ��8λ���봮��  0000 0000
		DELAY_MS(100);;
	}
}/**********************************************************************************************/
/*************************************************************
* �������� www.DoYoung.net
/*************************************************************/