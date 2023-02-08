using Wox.Plugin;
using Wox.Plugin.Logger;
using Translater.Utils;

namespace Translater
{
    public class TranslateHelper
    {
        public struct TranslateTarget
        {
            public string src;
            public string toLan;
        }
        public const string toLanSplit = "->";
        public bool inited => this.youdaoTranslater != null;
        private object initLock = new Object();
        private Youdao.YoudaoTranslater? youdaoTranslater;
        public TranslateHelper()
        {
            this.initTranslater();
        }
        public TranslateTarget ParseRawSrc(string src)
        {
            if (src.Contains(toLanSplit))
            {
                var srcArr = src.Split(toLanSplit);
                return new TranslateTarget
                {
                    src = srcArr.First().TrimEnd().TrimStart(),
                    toLan = srcArr.Last().TrimEnd().TrimStart()
                };
            }
            return new TranslateTarget
            {
                src = src,
                toLan = "AUTO"
            };
        }
        public List<ResultItem> QueryTranslate(string raw, string translateFrom = "user input")
        {
            var res = new List<ResultItem>();
            if (raw.Length == 0)
                return res;
            var target = ParseRawSrc(raw);
            string src = target.src;
            string toLan = target.toLan;

            try
            {
                var translateRes = youdaoTranslater!.translate(src, toLan);
                if (translateRes != null && translateRes.errorCode == 0)
                {
                    res.Add(new ResultItem
                    {
                        Title = translateRes.translateResult![0][0].tgt,
                        SubTitle = $"{src} [{translateRes.type}] [Translate form {translateFrom}]"
                    });
                    if (translateRes.smartResult != null)
                    {
                        translateRes.smartResult?.entries.each((s) =>
                        {
                            string t = s.Replace("\r\n", " ").TrimStart();
                            if (string.IsNullOrEmpty(t))
                                return;
                            res.Add(new ResultItem
                            {
                                Title = t,
                                SubTitle = "[smart result]"
                            });
                        });
                    }
                }
                else
                {
                    res.Add(new ResultItem
                    {
                        Title = raw,
                        SubTitle = $"can not translate {src} to {toLan}"
                    });
                }
            }
            catch (Exception err)
            {
                res.Add(new ResultItem
                {
                    Title = "some error happen!",
                    SubTitle = err.Message
                });
                Log.Error(err.ToString(), typeof(Translater));
            }
            return res;
        }

        public bool initTranslater()
        {
            lock (this.initLock)
            {
                if (this.youdaoTranslater != null)
                    return true;
                try
                {
                    youdaoTranslater = new Youdao.YoudaoTranslater();
                    return true;
                }
                catch (Exception err)
                {
                    Log.Warn(err.Message, typeof(Translater));
                    return false;
                }
            }
        }
    }
}