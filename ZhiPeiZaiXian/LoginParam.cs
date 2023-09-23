namespace ZhiPeiZaiXian;


public record LoginParam(
    string? account,
    string appKey,
    string? password,
    int sid,
    int type,
    string authOpenId,
    string authType
);



