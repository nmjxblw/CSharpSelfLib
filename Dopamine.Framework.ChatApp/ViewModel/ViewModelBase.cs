using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
namespace Dopamine.ChatApp
{
    /// <summary>
    /// 应用程序中所有ViewModel类的基类。
    /// 它提供对属性更改通知的支持
    /// 以及清理事件实例处理程序等资源。这个类是抽象的。
    /// </summary>
    public abstract class ViewModelBase : IDisposable, INotifyPropertyChanged
    {
        private readonly Dictionary<string, object> dic = new Dictionary<string, object>();


        protected ViewModelBase()
        {
            LocalCommand.CommandAction = (obj) => CommandFactoryMethod(obj);
        }



        #region INotifyPropertyChanged Members

        /// <summary>
        /// 当此对象的属性具有新值时引发。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">新的属性.</param>
        protected internal virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        ///用于设置具有相等属性值的泛型方法
        /// 检查并引发属性更改事件。
        /// </summary>
        protected internal void SetPropertyValue<T>(T value, ref T field, [CallerMemberName] string propertyName = "")
        {
            if (value != null && !value.Equals(field) || value == null && field != null)
            {
                field = value;
                if (!string.IsNullOrEmpty(propertyName))
                {
                    OnPropertyChanged(propertyName);
                }
            }
        }


        protected internal void SetProperty<T>(T value, [CallerMemberName] string propertyName = "")
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                if (dic.TryGetValue(propertyName, out object obj))
                {
                    if (obj == null)
                    {
                        dic[propertyName] = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                    }
                    else if (!obj.Equals(value))
                    {
                        dic[propertyName] = value;
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                    }


                }
                else
                {
                    dic.Add(propertyName, value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }

        protected internal T GetProperty<T>(T defValue, [CallerMemberName] string propertyName = "")
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return defValue;
            }
            else
            {
                if (dic.TryGetValue(propertyName, out object obj))
                    return (T)obj;
                else
                    return defValue;
            }
        }
        protected internal T GetProperty<T>([CallerMemberName] string propertyName = "")
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return default;
            }
            else
            {
                if (dic.TryGetValue(propertyName, out object obj))
                    return (T)obj;
                else
                    return default;
            }
        }



        #endregion // INotifyPropertyChanged Members

        /// <summary>
        /// 从应用程序中删除此对象时调用
        /// 并将接受垃圾收集。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// 子类可以重写此方法以执行
        /// 清理逻辑，例如删除事件实例处理程序。
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (PropertyChanged != null)
            {
                Delegate[] ds = PropertyChanged.GetInvocationList();
                foreach (Delegate d in ds)
                {
                    if (d is PropertyChangedEventHandler pd)
                    {
                        PropertyChanged -= pd;
                    }
                }
            }
            LocalCommand.CommandAction = null;
            LocalCommand = null;
        }

        #region 命令相关
        private BasicCommand localCommand;
        /// 控件命令
        /// <summary>
        /// 控件命令
        /// </summary>
        public BasicCommand LocalCommand
        {
            get
            {
                if (localCommand == null)
                {
                    localCommand = new BasicCommand();
                }
                return localCommand;
            }
            set => localCommand = value;
        }
        /// <summary>
        /// 命令工厂方法
        /// </summary>
        /// <param name="methodName"></param>
        public virtual void CommandFactoryMethod(string methodName)
        {
            try
            {
                //将方法添加到数据库进程去处理
                MethodInfo method = GetType().GetMethod(methodName);
                if (method == null)
                {
                    MessageBox.Show($"发送了无效命令，未找到方法{methodName}!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                method?.Invoke(this, new object[] { });
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.Message);
                Recorder.RecordError(e);
            }
        }
        #endregion


        /// <summary>
        /// 用于确保ViewModel对象被正确地垃圾收集。
        /// </summary>
        ~ViewModelBase()
        {
        }

    }
    /// <summary>
    /// 最简单的命令类
    /// </summary>
    public class BasicCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            { }
            remove
            { }
        }
        /// <summary>
        /// 执行外部定义的动作
        /// </summary>
        /// <param name="parameter">方法名称,即CommandParameter的值,该方法必须定义成公有方法,不然程序会找不到该方法,无法执行操作.</param>
        public void Execute(object parameter)
        {
            if (CommandAction != null)
            {
                if (parameter is string stringPara)
                {
                    if (!string.IsNullOrEmpty(stringPara))
                    {
                        try
                        {
                            CommandAction.Invoke(stringPara);
                        }
                        catch (Exception ex)
                        {
                            Recorder.RecordError(ex);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 命令下发时要执行的动作
        /// </summary>
        public Action<string> CommandAction { get; set; }
    }
}
