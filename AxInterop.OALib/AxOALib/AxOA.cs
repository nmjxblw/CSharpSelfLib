using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OALib;

namespace AxOALib;

[DesignTimeVisible(true)]
[Clsid("{18a295da-088e-42d1-be31-5028d7f9b9b5}")]
public class AxOA : AxHost
{
	private _DOA ocx;

	private AxOAEventMulticaster eventMulticaster;

	private ConnectionPointCookie cookie;

	[DispId(1)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual short BorderStyle
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("BorderStyle", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.BorderStyle;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("BorderStyle", ActiveXInvokeKind.PropertySet);
			}
			ocx.BorderStyle = value;
		}
	}

	[DispId(2)]
	[ComAliasName("System.UInt32")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual Color BorderColor
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("BorderColor", ActiveXInvokeKind.PropertyGet);
			}
			return AxHost.GetColorFromOleColor(ocx.BorderColor);
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("BorderColor", ActiveXInvokeKind.PropertySet);
			}
			ocx.BorderColor = AxHost.GetOleColorFromColor(value);
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(3)]
	public virtual string CaptionLabel
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("CaptionLabel", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.CaptionLabel;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("CaptionLabel", ActiveXInvokeKind.PropertySet);
			}
			ocx.CaptionLabel = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(4)]
	[ComAliasName("System.UInt32")]
	public virtual Color CaptionColor
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("CaptionColor", ActiveXInvokeKind.PropertyGet);
			}
			return AxHost.GetColorFromOleColor(ocx.CaptionColor);
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("CaptionColor", ActiveXInvokeKind.PropertySet);
			}
			ocx.CaptionColor = AxHost.GetOleColorFromColor(value);
		}
	}

	public AxOA()
		: base("18a295da-088e-42d1-be31-5028d7f9b9b5")
	{
		SetAboutBoxDelegate(AboutBox);
	}

	public virtual object GetIDispatch()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("GetIDispatch", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.GetIDispatch();
	}

	public virtual bool Open(string path)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("Open", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.Open(path);
	}

	public virtual bool Close()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("Close", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.Close();
	}

	public virtual bool CreateNew(string progId)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("CreateNew", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.CreateNew(progId);
	}

	public virtual bool OpenWebFile(string fileUrl)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("OpenWebFile", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.OpenWebFile(fileUrl);
	}

	public virtual bool HttpUploadFile(string serverUrl, string localFilePath)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("HttpUploadFile", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.HttpUploadFile(serverUrl, localFilePath);
	}

	public virtual bool HttpDownloadFile(string serverUrl, string localFileUrl)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("HttpDownloadFile", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.HttpDownloadFile(serverUrl, localFileUrl);
	}

	public virtual bool FTPUploadFile(string serverUrl, string localFilePath, string userName, string password)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("FTPUploadFile", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.FTPUploadFile(serverUrl, localFilePath, userName, password);
	}

	public virtual bool FTPDownloadFile(string serverUrl, string localFilePath, string userName, string password)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("FTPDownloadFile", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.FTPDownloadFile(serverUrl, localFilePath, userName, password);
	}

	public virtual void Save(string strPath)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("Save", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.Save(strPath);
	}

	public virtual void Print()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("Print", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.Print();
	}

	public virtual bool SaveLocalDialog()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("SaveLocalDialog", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.SaveLocalDialog();
	}

	public virtual bool OpenLocalDialog()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("OpenLocalDialog", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.OpenLocalDialog();
	}

	public virtual bool IsOpen()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("IsOpen", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.IsOpen();
	}

	public virtual void SaveWebFile(string serverUrl)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("SaveWebFile", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.SaveWebFile(serverUrl);
	}

	public virtual bool DoOleCommand(int dwOleCmdID, int dwOptions, ref object vInParam, ref object vInOutParam)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("DoOleCommand", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.DoOleCommand(dwOleCmdID, dwOptions, ref vInParam, ref vInOutParam);
	}

	public virtual bool IsDirty()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("IsDirty", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.IsDirty();
	}

	public virtual void ShowToolbars(bool bShow)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("ShowToolbars", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.ShowToolbars(bShow);
	}

	public virtual bool GetToolbarsIsShow()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("GetToolbarsIsShow", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.GetToolbarsIsShow();
	}

	public virtual void AboutBox()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("AboutBox", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.AboutBox();
	}

	protected override void CreateSink()
	{
		try
		{
			eventMulticaster = new AxOAEventMulticaster(this);
			cookie = new ConnectionPointCookie(ocx, eventMulticaster, typeof(_DOAEvents));
		}
		catch (Exception)
		{
		}
	}

	protected override void DetachSink()
	{
		try
		{
			cookie.Disconnect();
		}
		catch (Exception)
		{
		}
	}

	protected override void AttachInterfaces()
	{
		try
		{
			ocx = (_DOA)GetOcx();
		}
		catch (Exception)
		{
		}
	}
}
