using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OALib;

[ComImport]
[TypeLibType(4112)]
[InterfaceType(2)]
[Guid("16EFE4A7-6641-459C-8BB3-B02AC6F088F4")]
public interface _DOA
{
	[DispId(1)]
	short BorderStyle
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		set;
	}

	[DispId(2)]
	[ComAliasName("stdole.OLE_COLOR")]
	uint BorderColor
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(2)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(2)]
		set;
	}

	[DispId(3)]
	string CaptionLabel
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(3)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(3)]
		set;
	}

	[DispId(4)]
	[ComAliasName("stdole.OLE_COLOR")]
	uint CaptionColor
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(4)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(4)]
		set;
	}

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(5)]
	[return: MarshalAs(UnmanagedType.IDispatch)]
	object GetIDispatch();

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(6)]
	bool Open([MarshalAs(UnmanagedType.BStr)] string Path);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(7)]
	bool Close();

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(8)]
	bool CreateNew([MarshalAs(UnmanagedType.BStr)] string ProgId);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(9)]
	bool OpenWebFile([MarshalAs(UnmanagedType.BStr)] string FileUrl);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(10)]
	bool HttpUploadFile([MarshalAs(UnmanagedType.BStr)] string ServerUrl, [MarshalAs(UnmanagedType.BStr)] string LocalFilePath);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(11)]
	bool HttpDownloadFile([MarshalAs(UnmanagedType.BStr)] string ServerUrl, [MarshalAs(UnmanagedType.BStr)] string LocalFileUrl);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(12)]
	bool FTPUploadFile([MarshalAs(UnmanagedType.BStr)] string ServerUrl, [MarshalAs(UnmanagedType.BStr)] string LocalFilePath, [MarshalAs(UnmanagedType.BStr)] string UserName, [MarshalAs(UnmanagedType.BStr)] string Password);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(13)]
	bool FTPDownloadFile([MarshalAs(UnmanagedType.BStr)] string ServerUrl, [MarshalAs(UnmanagedType.BStr)] string LocalFilePath, [MarshalAs(UnmanagedType.BStr)] string UserName, [MarshalAs(UnmanagedType.BStr)] string Password);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(14)]
	void Save([MarshalAs(UnmanagedType.BStr)] string strPath);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(15)]
	void Print();

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(16)]
	bool SaveLocalDialog();

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(17)]
	bool OpenLocalDialog();

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(18)]
	bool IsOpen();

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(19)]
	void SaveWebFile([MarshalAs(UnmanagedType.BStr)] string ServerUrl);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(20)]
	bool DoOleCommand(int dwOleCmdID, int dwOptions, [MarshalAs(UnmanagedType.Struct)] ref object vInParam, [MarshalAs(UnmanagedType.Struct)] ref object vInOutParam);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(21)]
	bool IsDirty();

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(22)]
	void ShowToolbars(bool bShow);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(23)]
	bool GetToolbarsIsShow();

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(-552)]
	void AboutBox();
}
