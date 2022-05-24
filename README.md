<h3 align="center">Stage: Init</h3>

---

Integrate your [Supabase](https://supabase.io) projects with Unity.

Combined [spabase-csharp repo](https://github.com/supabase-community/supabase-csharp) into one easy to install Unity package.

## Installation

### Install via git URL
In Unity click Package Manager => "+" button -> Install via git URL

You can install cs files only: https://github.com/kamyker/supabase-unity.git?path=Unity

Or whole builder to be able to generate .meta files in Unity: https://github.com/kamyker/supabase-unity.git

Then you may have to add dependand dlls that aren't already in your project. They could be installed by other packages. Simply check Unity editor log and add what's needed. For ex:

```
Packages\supabase-unity\Unity\supabase-cloned\supabase-csharp\modules\realtime-csharp\Realtime\Socket.cs(36,17):
error CS0246: The type or namespace name 'WebsocketClient' could not be found
(are you missing a using directive or an assembly reference?)
```

Means you have to install https://github.com/kamyker/supabase-unity.git?path=UnityDlls~/Websocket.Client (via git URL in package manager). Check UnityDlls~ folder in this repo to see what dlls are available.
