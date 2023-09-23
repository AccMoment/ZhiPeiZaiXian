namespace ZhiPeiZaiXian;

public record User(
    bool success,
    string messageCode,
    string message,
    Data data
);

public record Data(
    string accessToken,
    string appKey,
    string userCode,
    int sid,
    string refreshToken,
    bool cloginB
);

