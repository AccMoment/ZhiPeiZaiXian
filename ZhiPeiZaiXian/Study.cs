namespace ZhiPeiZaiXian;

public record Study(
    int id,
    int class_id,
    int uid,
    string exam_score,
    string practice_score,
    string create_time,
    string is_exam_score,
    string is_practice_score,
    string type,
    string subsidy,
    string labour,
    string group_id,
    string is_questionnaire,
    string questionnaire_time,
    string contract_start_time,
    string contract_end_time,
    string status,
    string invalid_remark,
    string cer_code,
    string verify_status,
    string company_name,
    string company_code,
    FrontendClass frontendClass,
    CourseMap[] courseMap,
    Assess[] assess,
    int satisfied,
    int demand,
    double progress
);

public record FrontendClass(
    string id,
    string name,
    string create_user,
    string course_category,
    string type,
    string course_time,
    double course_period,
    string status,
    string policy_id,
    string start_time,
    string finish_time,
    string category_level,
    string category_id,
    StatusName statusName,
    TypeName typeName,
    CreateUser createUser,
    PolicyInfo policyInfo,
    TrainCategory trainCategory,
    TrainJob trainJob,
    string category
);

public record StatusName(
    string key,
    string name
);

public record TypeName(
    string key,
    string name
);

public record CreateUser(
    string id,
    string name,
    string avatar,
    string type
);

public record PolicyInfo(
    string id,
    string title,
    string study_publish,
    string unit,
    string test_type,
    string student_type,
    string staff_type,
    string coupons_support,
    string append_student,
    string real_auth,
    string supervise_open,
    string class_num,
    string need_audit,
    string total_period,
    string identity_verify
);

public record TrainCategory(
    string id,
    string pid,
    string name,
    object parentCategory
);

public record TrainJob(
    string id,
    string name
);

public record CourseMap(
    string id,
    string class_id,
    string uid,
    string course_id,
    string type,
    string create_time,
    string score,
    string status,
    double progress,
    string year,
    CourseType courseType,
    StudyCourse course
);

public record CourseType(
    string key,
    string name
);

public record StudyCourse(
    string id,
    string bid,
    string title,
    string image,
    string total_period,
    string total_time,
    string total_video,
    Source source
);

public record Source(
    string id,
    string name,
    string alias_name,
    string logo,
    Seller seller
);

public record Seller(
    string organization_id,
    string company_name,
    string company_alias_name,
    string company_contact,
    string contact_mobile,
    string contact_name,
    string type,
    string content,
    string audit_content_status
);

public record Assess(
    string id,
    string policy_id,
    string name,
    string type,
    string way,
    string upload_require,
    object ext,
    string ext_limit,
    string size,
    string number,
    string period,
    object config,
    int study_user,
    string image,
    object[] template_id,
    int study,
    int valid,
    int progress,
    string class_user_assess_id,
    string score,
    string remark,
    string status,
    object[] files
);

