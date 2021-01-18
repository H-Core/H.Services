using System.Linq;
using H.Hooks;
using Key = H.Core.Key;
using Keys = H.Core.Keys;

namespace H.Services.Extensions
{
    internal static class KeyboardHookEventArgsExtensions
    {
        public static Keys ToKeys(this KeyboardHookEventArgs args)
        {
            return new(args.Keys.Values
                .Select(static key => (Key)(int)key)
                .ToArray());
        }
    }
}
