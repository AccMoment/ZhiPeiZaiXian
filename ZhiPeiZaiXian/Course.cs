namespace ZhiPeiZaiXian;

public record Courses(
    int company_is_buy,
    int is_task,
    Course[] course,
    bool bj_free_see,
    int is_test
);

public record Course(
    string id,
    string course_id,
    string title,
    string description,
    string sort,
    string hide,
    string create_time,
    string update_time,
    object sections,
    Units[] units,
    string order_number
);

public record Units(
    string id,
    string bid,
    string open_id,
    string chapter_id,
    string section_id,
    string type,
    string title,
    string description,
    string total_time,
    string total_question,
    string sort,
    string hide,
    string create_time,
    string update_time,
    string is_see,
    TypeName typeName,
    object[] files,
    string status,
    string remarks,
    StatusInfo statusInfo,
    string order_number,
    object train,
    object lastTrain,
    object unitMap,
    int lastUnitId,
    int progress_time,
    double progress
);

public record StatusInfo(
    string key,
    string name
);

