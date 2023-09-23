// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using ZhiPeiZaiXian;

const string loginUrl = "https://api.cloud.wozhipei.com/auth/user/v1/login";
const string studyUrl = "https://apif.wozhipei.com/student-center/study";
const string logoutUrl = "https://api.cloud.wozhipei.com/auth/user/v1/logout";
const string userFile = "User.txt";
const string logFile = "log.txt";

if (!File.Exists(logFile)) File.Create(logFile).Dispose();
if (!File.Exists(userFile)) File.Create(userFile).Dispose();

var httpClient = new HttpClient
{
    Timeout = new TimeSpan(0, 0, 1, 0)
};
try
{
    
    var speed = 60;
    Console.Write("请输入观看速度(默认为60s,指的是60s请求一次用于视频进度更新,推荐选择不动，过小可能会导致账号被检测到使用违规软件,留空为默认):");
    var s = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(s))
    {
        while (!int.TryParse(s, out speed))
        {
            Console.WriteLine("请输入正确数字!");
            s = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(s)) break;
        }
    }
    
    
    var userInfo = File.ReadAllText(userFile);
    string token;

    if (string.IsNullOrWhiteSpace(userInfo))
    {
        token = await Login();
        Console.WriteLine("登录中...");
    }
    else
    {
        Console.WriteLine("正在获取账号信息...");
        var settings = JsonSerializer.Deserialize<Settings>(userInfo, new JsonSerializerOptions
        {
            TypeInfoResolver = CommonContext.Default
        })!;
        await UpdateToken(settings);
        token = settings.Token!;
        Console.WriteLine("已获取账号信息!");
        Console.WriteLine("登录中...");
    }

   

    var updateRequest = new HttpRequestMessage(HttpMethod.Post, "https://apif.wozhipei.com/users/update-access-token")
    {
        Content = new StringContent(JsonSerializer.Serialize(new EmptyBody(), CommonContext.Default.EmptyBody))
    };
    AddHeader(updateRequest, token);
    updateRequest.Headers.Add("authorization-platform", token);
    var updateResponse = await httpClient.SendAsync(updateRequest);
    // if(updateResponse.StatusCode == HttpStatusCode.OK) Console.WriteLine("刷新成功!");


    var organizationRequest =
        new HttpRequestMessage(HttpMethod.Get, "https://apif.wozhipei.com/agency-center/user-organization");
    AddHeader(organizationRequest, token);
    organizationRequest.Headers.Add("authorization-platform", token);
    var organizationResponse = await httpClient.SendAsync(organizationRequest);
    // if(updateResponse.StatusCode == HttpStatusCode.OK) Console.WriteLine("刷新成功!");


    var studyRequest = new HttpRequestMessage(HttpMethod.Get, studyUrl);
    AddHeader(studyRequest, token);
    studyRequest.Headers.Add("authorization-platform", token);
    var studyResponseMessage = await httpClient.SendAsync(studyRequest);

    Console.WriteLine("登录成功");

    Console.WriteLine("正在获取课程中..");


    var study = JsonSerializer.Deserialize<List<Study>>(await studyResponseMessage.Content.ReadAsStringAsync(),
        new JsonSerializerOptions
        {
            TypeInfoResolver = CommonContext.Default,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        })?[0];

    Console.WriteLine($"获取到课程名:{study?.courseMap[0].course.title}");
    var courseId = study?.courseMap[0].course.id;
    var classId = study?.class_id;
    var uid = study?.uid;

// var courseDetailRequest = new HttpRequestMessage(HttpMethod.Get,
//     $"https://apif.wozhipei.com/courses/{courseId}?class_id={classId}")
// {
//     Headers =
//     {
//         Authorization = new AuthenticationHeaderValue("Bearer", token),
//         Referrer = new Uri($"https://apif.wozhipei.com/courses/{courseId}?class_id={classId}"),
//         Host = "apif.wozhipei.com"
//     }
// };
// courseDetailRequest.Headers.Add("Origin", "https://jiangxi.zhipeizaixian.com");
// var courseDetail = await (await httpClient.SendAsync(courseDetailRequest
//   )).Content.ReadFromJsonAsync<JsonObject>();
// Console.WriteLine($"获取总课时为:{courseDetail?["total_video"]}");
// Console.WriteLine($"获取总进度为:{courseDetail?["total_period"]}");

    var coursesRequest = new HttpRequestMessage(HttpMethod.Get,
        $"https://apif.wozhipei.com/courses/test-preview?course_id={courseId}&class_id={classId}");
    AddHeader(coursesRequest, token);
    var coursesResponseMessage = await httpClient.SendAsync(
        coursesRequest);
    var courses = JsonSerializer.Deserialize<Courses?>(await coursesResponseMessage.Content.ReadAsStringAsync(),
        new JsonSerializerOptions
        {
            TypeInfoResolver = CommonContext.Default,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        });
    var coursesId = courses?.course.ToList().FindAll(f => f.units[0].progress != 1d);
    Console.WriteLine($"视频总数为:{courses?.course.Length}");
    Console.WriteLine($"当前未完成视频总数为:{coursesId?.Count}");
    Console.WriteLine("即将开始学习...");
    var competeTask = 0;
    while (competeTask != coursesId?.Count)
    {
        var unitsId = Convert.ToInt32(coursesId?[competeTask].units[0].id);

        var videoRequest = new HttpRequestMessage(HttpMethod.Get,
            $"https://apif.wozhipei.com/course-units/{unitsId}?class_id={classId}");
        AddHeader(videoRequest, token);

        var videoResponseMessage = await httpClient.SendAsync(videoRequest);
        var videoJsonObject = JsonSerializer.Deserialize<JsonObject>(
            await videoResponseMessage.Content.ReadAsStringAsync(),
            new JsonSerializerOptions
            {
                TypeInfoResolver = CommonContext.Default,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            });
        var videoId = videoJsonObject?["video"]?["id"]?.ToString();

        var videoTime = Convert.ToInt32(coursesId?[competeTask].units[0].total_time);
        Console.WriteLine($"开始学习:{coursesId?[competeTask].title}");
        var url =
            $"https://apistudy.wozhipei.com/studies/study?video_id={videoId}&u={uid}&time={videoTime}&unit_id={unitsId}&class_id={classId}";

        while (true)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            AddHeader(request, token);
            var response = await httpClient.SendAsync(request);


            var result = JsonSerializer.Deserialize<JsonObject>(await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions
                {
                    TypeInfoResolver = CommonContext.Default,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                });
            var competeTime = result?["video_time"]?.GetValue<int>()!;
            var totalTime = Convert.ToInt32(result?["count_time"]?.GetValue<string>()!);
            var code = result?["code"]?.GetValue<int>();
            switch (code)
            {
                case 3002:
                    Console.WriteLine("你可能被系统检测到违规操作，请正常观看一段时间视频后再尝试本软件");
                    return;
                case 2003:
                    // Console.WriteLine("需要人脸识别，尝试使用缓存图片");
                    // var fs = new FileStream("image.txt", FileMode.Open, FileAccess.ReadWrite);
                    // var sr = new StreamReader(fs);
                    // var image = sr.ReadToEnd();
                    // sr.Dispose();
                    // fs.Dispose();
                    // await Task.Delay(2000);
                    // if (string.IsNullOrWhiteSpace(image))
                    // {
                    Console.WriteLine("请前往对应课程进行人脸认证之后，再按下回车键..");
                    Console.ReadKey();
                    //     break;
                    // }

                    // supervisesRequest = new HttpRequestMessage(HttpMethod.Post, "https://apif.wozhipei.com/supervises")
                    // {
                    //     // Headers =
                    //     // {
                    //     //     Authorization = new AuthenticationHeaderValue("Bearer", token)
                    //     // },
                    //     Content = new StringContent(JsonSerializer.Serialize(new
                    //     {
                    //         baseImage = image,
                    //         class_id = classId.ToString(),
                    //         course_id = courseId,
                    //         unit_id = unitsId.ToString()
                    //     }))
                    // };
                    // supervisesRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    // AddHeader(supervisesRequest, token);
                    // if (httpClient.SendAsync(supervisesRequest).Result.StatusCode == HttpStatusCode.OK)
                    // {
                    //     Console.WriteLine("识别成功! 继续运行...");
                    // }
                    // else
                    // {
                    //     Console.WriteLine("识别失败,请在网页上进行人脸识别，再按下回车键..");
                    //     Console.ReadKey();
                    // }

                    await Task.Delay(100);
                    break;

                case 2002:
                    Console.WriteLine("需要人机验证，正在获取图片");
                    HttpResponseMessage validateResponse;
                    do
                    {
                        var codeRequest =
                            new HttpRequestMessage(HttpMethod.Get, "https://apif.wozhipei.com/supervises/code");
                        AddHeader(codeRequest, token);
                        var base64Image =
                            (await (await httpClient.SendAsync(codeRequest)).Content.ReadAsStringAsync())
                            .Replace("data:image/png;base64,", "")
                            .Replace("\"", "")
                            .Trim();
                        var codeImage = "image.png";
                        ConvertBase64ToImage(codeImage, base64Image);
                        Console.WriteLine("已获取到图片...");
                        var process = new Process();
                        //设置文件名，此处为图片的真实路径+文件名 
                        process.StartInfo.FileName = Path.Combine(Environment.CurrentDirectory, codeImage);
                        //此为关键部分。设置进程运行参数，此时为最大化窗口显示图片。 
                        process.StartInfo.Arguments = @"rundll32.exe C:\WINDOWS\system32\shimgvw.dll";
                        //此项为是否使用Shell执行程序，因系统默认为true，此项也可不设，但若设置必须为true 
                        process.StartInfo.UseShellExecute = true;
                        //此处可以更改进程所打开窗体的显示样式，可以不设 
                        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        process.Start();
                        process.Close();
                        Console.Write("请输入验证码:");
                        var validateCode = Console.ReadLine();
                        var validateRequest = new HttpRequestMessage(HttpMethod.Post,
                            "https://apif.wozhipei.com/supervises/smart-new")
                        {
                            // Headers =
                            // {
                            //     Authorization = new AuthenticationHeaderValue("Bearer", token)
                            // },
                            Content = new StringContent(JsonSerializer.Serialize(new ValidateCode
                            {
                                code = validateCode,
                                class_id = classId.ToString(),
                                course_id = courseId,
                                unit_id = unitsId.ToString()
                            }, new JsonSerializerOptions
                            {
                                TypeInfoResolver = CommonContext.Default
                            }))
                        };
                        validateRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        AddHeader(validateRequest, token);
                        validateResponse = await httpClient.SendAsync(validateRequest);
                        if (validateResponse.StatusCode == HttpStatusCode.OK)
                        {
                            Console.WriteLine("识别成功,继续运行..");
                        }
                        else
                        {
                            Console.WriteLine("验证码识别失败,重新获取验证码");
                        }
                    } while (validateResponse.StatusCode != HttpStatusCode.OK);

                    break;
                case 555:
                    Console.WriteLine($"任务进行中({(double)competeTime * 100 / totalTime:f2}%)");
                    await Task.Delay(speed * 1000);
                    break;
                case 3003:
                    Console.WriteLine("可能学习时长超过限制了，请隔天再试吧");
                    Console.ReadKey();
                    return;
                default:
                    Console.WriteLine("未知错误，请报告开发者!");
                    Console.ReadKey();
                    return;
            }

            if (competeTime == totalTime) break;
        }

        Console.WriteLine($"完成课时:{coursesId?[competeTask++].title}");
        await Task.Delay(1000);
    }

    Console.WriteLine("已完成！");
    Console.ReadKey();
}
catch (Exception e)
{
    File.AppendAllText(logFile, e.Message);
    Console.WriteLine(e.Message);
    Console.ReadKey();
    return;
}

return;


void AddHeader(HttpRequestMessage message, string? token)
{
    message.Headers.Add("Origin", "https://jiangxi.zhipeizaixian.com");
    message.Headers.Add("X-Client-Type", "pc");
    message.Headers.Add("X-User-Type", "1");
    message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
}


void ConvertBase64ToImage(string fileName, string base64Str)
{
    var bytes = Convert.FromBase64String(base64Str);
    using var msIn = new MemoryStream(bytes);
#pragma warning disable CA1416
    var pic = Image.FromStream(msIn);
    using var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
    pic.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
#pragma warning restore CA1416
}

async Task<string> Login()
{
    User? user;
    do
    {
        Console.Write("请输入账号:");
        var account = Console.ReadLine();
        Console.Write("请输入密码:");
        var password = Console.ReadLine();
        user = await SendUserRequest(account, password);

        if (user?.success == false)
        {
            Console.WriteLine($"{user.message},请重新填写账号密码!");
        }
        else
        {
            // Console.WriteLine("登录成功!");
            var setting = new Settings
            {
                Account = account,
                Password = password,
                Token = user?.data.accessToken
            };
            File.WriteAllText(userFile,
                JsonSerializer.Serialize(setting,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true, TypeInfoResolver = CommonContext.Default
                    }));
            return user?.data.accessToken!;
        }
    } while (user.success == false);

    return string.Empty;
}


async Task<User?> SendUserRequest(string? account, string? password)
{
    var login = new LoginParam
    (
        account: account,
        appKey: "WEB",
        authOpenId: "",
        authType: "",
        password: password,
        sid: 1018,
        type: 1
    );
    // httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    var loginContent = new StringContent(JsonSerializer.Serialize(login, new JsonSerializerOptions()
    {
        TypeInfoResolver = CommonContext.Default
    }))
    {
        Headers =
        {
            ContentType = new MediaTypeHeaderValue("application/json")
        }
    };
    var request = new HttpRequestMessage(HttpMethod.Post, loginUrl)
    {
        Content = loginContent
    };

    var message = await httpClient.SendAsync(request);
    var user = JsonSerializer.Deserialize<User>(await message.Content.ReadAsStringAsync(),
        new JsonSerializerOptions
        {
            TypeInfoResolver = CommonContext.Default,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        });
    return user;
}

async Task UpdateToken(Settings settings)
{
    var studyRequest = new HttpRequestMessage(HttpMethod.Get, studyUrl);
    AddHeader(studyRequest, settings.Token);
    studyRequest.Headers.Add("authorization-platform", settings.Token);
    var studyResponseMessage = await httpClient.SendAsync(studyRequest);
    if (studyResponseMessage.StatusCode != HttpStatusCode.OK)
    {
        var user = await SendUserRequest(settings.Account, settings.Password);
        settings.Token = user?.data.accessToken;
    }
}