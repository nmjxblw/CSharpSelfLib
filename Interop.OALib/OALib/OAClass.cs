using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OALib;

[ComImport]
[ComSourceInterfaces("OALib._DOAEvents\0\0")]
[ClassInterface(0)]
[Guid("18A295DA-088E-42D1-BE31-5028D7F9B9B5")]
[TypeLibType(34)]
public class OAClass : _DOA, OA, _DOAEvents_Event
{
	[DispId(1)]
	public virtual extern short BorderStyle
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(1)]
		set;
	}

	[ComAliasName("stdole.OLE_COLOR")]
	[DispId(2)]
	public virtual extern uint BorderColor
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(2)]
		get;
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[DispId(2)]
		set;
	}

	[DispId(3)]
	public virtual extern string CaptionLabel
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
	public virtual extern uint CaptionColor
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
	public virtual extern object GetIDispatch();

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(6)]
	public virtual extern bool Open([MarshalAs(UnmanagedType.BStr)] string Path);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(7)]
	public virtual extern bool Close();

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(8)]
	public virtual extern bool CreateNew([MarshalAs(UnmanagedType.BStr)] string ProgId);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(9)]
	public virtual extern bool OpenWebFile([MarshalAs(UnmanagedType.BStr)] string FileUrl);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(10)]
	public virtual extern bool HttpUploadFile([MarshalAs(UnmanagedType.BStr)] string ServerUrl, [MarshalAs(UnmanagedType.BStr)] string LocalFilePath);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(11)]
	public virtual extern bool HttpDownloadFile([MarshalAs(UnmanagedType.BStr)] string ServerUrl, [MarshalAs(UnmanagedType.BStr)] string LocalFileUrl);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(12)]
	public virtual extern bool FTPUploadFile([MarshalAs(UnmanagedType.BStr)] string ServerUrl, [MarshalAs(UnmanagedType.BStr)] string LocalFilePath, [MarshalAs(UnmanagedType.BStr)] string UserName, [MarshalAs(UnmanagedType.BStr)] string Password);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(13)]
	public virtual extern bool FTPDownloadFile([MarshalAs(UnmanagedType.BStr)] string ServerUrl, [MarshalAs(UnmanagedType.BStr)] string LocalFilePath, [MarshalAs(UnmanagedType.BStr)] string UserName, [MarshalAs(UnmanagedType.BStr)] string Password);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(14)]
	public virtual extern void Save([MarshalAs(UnmanagedType.BStr)] string strPath);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(15)]
	public virtual extern void Print();

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(16)]
	public virtual extern bool SaveLocalDialog();

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(17)]
	public virtual extern bool OpenLocalDialog();

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(18)]
	public virtual extern bool IsOpen();

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(19)]
	public virtual extern void SaveWebFile([MarshalAs(UnmanagedType.BStr)] string ServerUrl);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(20)]
	public virtual extern bool DoOleCommand(int dwOleCmdID, int dwOptions, [MarshalAs(UnmanagedType.Struct)] ref object vInParam, [MarshalAs(UnmanagedType.Struct)] ref object vInOutParam);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(21)]
	public virtual extern bool IsDirty();

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(22)]
	public virtual extern void ShowToolbars(bool bShow);

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(23)]
	public virtual extern bool GetToolbarsIsShow();

	[MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
	[DispId(-552)]
	public virtual extern void AboutBox();
}
