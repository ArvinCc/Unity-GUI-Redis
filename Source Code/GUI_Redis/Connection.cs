﻿using UnityEngine;
using Renci.SshNet;
using System;
using Renci.SshNet.Common;
using ServiceStack.Redis;

namespace JEngine.Redis
{
    public class Connection
    {
        public string SQL_IP = "127.0.0.1";//Redis 数据库IP
        public uint SQL_Port = 6379;//Redis 数据库端口
        public string SQL_Password = "your_db_password";//Redis 数据库密码
        public int SQL_DB = 0;//数据库

        public bool Debug;//调试模式

        #region SSH连接部分
        public bool ConnectThroughSSH = true;
        public string SSH_Host = "127.0.0.1";   //SSH ip 地址
        public int SSH_Port = 22;    //SSH 端口（一般情况下都是22端口）
        public string SSH_User = "root";    //SSH 用户
        public string SSH_Password = "your_password";    //SSH 密码
        #endregion

        /// <summary>
        /// 连接Redis，执行success，错误回调error
        /// </summary>
        /// <param name="success"></param>
        /// <param name="error"></param>
        public void Connect(Action<RedisClient> success = null, Action error = null)
        {
            //如果通过SSH连接Redis
            if (ConnectThroughSSH)
            {
                SSH_Connect(success, error);
            }
            else
            {
                Direct_Connect();
            }
        }

        /// <summary>
        /// 直连
        /// </summary>
        private void Direct_Connect(Action<RedisClient> success = null, Action error = null)
        {
            //开启计时器
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            var redis = new RedisClient(SQL_IP, (int)SQL_Port, SQL_Password, SQL_DB);
            try
            {
                redis.GetRandomKey();
                stopwatch.Stop();

                if (Debug)
                {
                    if (RedisWindow.Language == Language.中文)
                    {
                        Core.Log("Redis连接成功 (耗时" + stopwatch.ElapsedMilliseconds + "ms)");
                    }
                    else
                    {
                        Core.Log("Connected Redis Successfully (Spent " + stopwatch.ElapsedMilliseconds + "ms)");
                    }
                }


                //执行
                if (success != null)
                {
                    stopwatch.Reset();
                    stopwatch.Start();
                    success(redis);
                    stopwatch.Stop();
                    if (Debug)
                    {
                        if (RedisWindow.Language == Language.中文)
                        {
                            Core.LogWarning("任务完成(耗时" + stopwatch.ElapsedMilliseconds + "ms)");
                        }
                        else
                        {
                            Core.LogWarning("Task Completed (Spent " + stopwatch.ElapsedMilliseconds + "ms)");
                        }
                    }
                }

            }
            catch (Exception e)
            {
                if (RedisWindow.Language == Language.中文)
                {
                    Core.LogError("无法连接Redis: " + e.Message);
                }
                else
                {
                    Core.LogError("Unable to connect to Redis: " + e.Message);
                }
                error?.Invoke();
            }
        }


        /// <summary>
        /// SSH 连接（推荐）
        /// </summary>
        private void SSH_Connect(Action<RedisClient> success = null, Action error = null)
        {
            //开启计时器
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            //创建SSH连接
            using (var client = new SshClient(SSH_Host, SSH_Port, SSH_User, SSH_Password))
            {
                try
                {
                    client.HostKeyReceived += delegate (object sender, HostKeyEventArgs e) { e.CanTrust = true; };
                    client.Connect();
                }
                catch (Exception e)
                {
                    if (RedisWindow.Language == Language.中文)
                    {
                        Core.LogWarning("无法通过SSH连接服务器: " + e.Message);
                    }
                    else
                    {
                        Core.LogWarning("Unable to connect to SSH: " + e.Message);
                    }
                    error?.Invoke();
                    return;
                }

                if (!client.IsConnected)
                {
                    if (RedisWindow.Language == Language.中文)
                    {
                        Core.LogWarning("无法通过SSH连接服务器，请确保端口开放");
                    }
                    else
                    {
                        Core.LogWarning("Unable to connect to SSH, make sure the port is avaliable");
                    }
                    error?.Invoke();
                    return;
                }

                stopwatch.Stop();
                if (Debug)
                {
                    if (RedisWindow.Language == Language.中文)
                    {
                        Core.Log("SSH连接成功 (耗时" + stopwatch.ElapsedMilliseconds + "ms)");
                    }
                    else
                    {
                        Core.Log("Successfully connected SSH (Spent " + stopwatch.ElapsedMilliseconds + "ms)");
                    }
                }

                var port = new ForwardedPortLocal("127.0.0.1", SQL_Port, "127.0.0.1", SQL_Port);
                client.AddForwardedPort(port);
                port.Start();

                stopwatch.Reset();
                stopwatch.Start();
                var redis = new RedisClient(SQL_IP, (int)SQL_Port, SQL_Password, SQL_DB);
                try
                {
                    redis.GetRandomKey();
                    stopwatch.Stop();

                    if (Debug)
                    {
                        if (RedisWindow.Language == Language.中文)
                        {
                            Core.Log("Redis连接成功 (耗时" + stopwatch.ElapsedMilliseconds + "ms)");
                        }
                        else
                        {
                            Core.Log("Connected Redis Successfully (Spent " + stopwatch.ElapsedMilliseconds + "ms)");
                        }
                    }


                    //执行
                    if (success != null)
                    {
                        stopwatch.Reset();
                        stopwatch.Start();
                        success(redis);
                        stopwatch.Stop();
                        if (Debug)
                        {
                            if (RedisWindow.Language == Language.中文)
                            {
                                Core.LogWarning("任务完成(耗时" + stopwatch.ElapsedMilliseconds + "ms)");
                            }
                            else
                            {
                                Core.LogWarning("Task Completed (Spent " + stopwatch.ElapsedMilliseconds + "ms)");
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    if (RedisWindow.Language == Language.中文)
                    {
                        Core.LogError("无法连接Redis: " + e.Message);
                    }
                    else
                    {
                        Core.LogError("Unable to connect to Redis: " + e.Message);
                    }
                    error?.Invoke();
                }
            }
        }
    }

    public class Core
    {
        public static void Log(object Message)
        {
            Debug.Log("[GUI-Redis] " + Message);
        }

        public static void LogWarning(object Message)
        {
            Debug.LogWarning("[GUI-Redis] " + Message);
        }

        public static void LogError(object Message)
        {
            Debug.LogError("[GUI-Redis] " + Message);
        }
    }
}