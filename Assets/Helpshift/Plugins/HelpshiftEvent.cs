using System;

namespace Helpshift
{
    /// <summary>
    /// Constants class for event names and their data keys
    /// </summary>
    public class HelpshiftEvent
    {
        public const string WIDGET_TOGGLE = "widgetToggle";
        public const string DATA_SDK_VISIBLE = "visible";

        public const string CONVERSATION_START = "conversationStart";
        public const string DATA_MESSAGE = "message";

        public const string MESSAGE_ADD = "messageAdd";
        public const string DATA_MESSAGE_TYPE = "type";
        public const string DATA_MESSAGE_BODY = "body";
        public const string DATA_MESSAGE_TYPE_ATTACHMENT = "attachment";
        public const string DATA_MESSAGE_TYPE_TEXT = "text";

        public const string CSAT_SUBMIT = "csatSubmit";
        public const string DATA_CSAT_RATING = "rating";
        public const string DATA_ADDITIONAL_FEEDBACK = "additionalFeedback";

        public const string CONVERSATION_STATUS = "conversationStatus";
        public const string DATA_LATEST_ISSUE_ID = "latestIssueId";
        public const string DATA_LATEST_ISSUE_PUBLISH_ID = "latestIssuePublishId";
        public const string DATA_IS_ISSUE_OPEN = "open";

        public const string CONVERSATION_END = "conversationEnd";

        public const string CONVERSATION_REJECTED = "conversationRejected";

        public const string CONVERSATION_RESOLVED = "conversationResolved";

        public const string CONVERSATION_REOPENED = "conversationReopened";


        public const string SDK_SESSION_STARTED = "helpshiftSessionStarted";

        public const string SDK_SESSION_ENDED = "helpshiftSessionEnded";

        public const string RECEIVED_UNREAD_MESSAGE_COUNT = "receivedUnreadMessageCount";
        public const string DATA_MESSAGE_COUNT = "count";
        public const string DATA_MESSAGE_COUNT_FROM_CACHE = "fromCache";

        public const string ACTION_CLICKED = "userClickOnAction";
        public const string DATA_ACTION = "actionType";
        public const string DATA_ACTION_TYPE = "actionData";
        public const string DATA_ACTION_TYPE_CALL = "call";
        public const string DATA_ACTION_TYPE_LINK = "link";

        public const string AGENT_MESSAGE_RECEIVED = "agentMessageReceived";
        public const string DATA_PUBLISH_ID = "publishId";
        public const string DATA_CREATED_TIME = "createdTs";
        public const string DATA_ATTACHMENTS = "attachments";
        public const string DATA_URL = "url";
        public const string DATA_CONTENT_TYPE = "contentType";
        public const string DATA_FILE_NAME = "fileName";
        public const string DATA_SIZE = "size";
        public const string DATA_MESSAGE_TYPE_APP_REVIEW_REQUEST = "app_review_request";
        public const string DATA_MESSAGE_TYPE_SCREENSHOT_REQUEST = "screenshot_request";

        // User identity events
        public const string ATTRIBUTES_DATA_INVALID = "attributesDataInvalid";
        public const string MASTER_ATTRIBUTES_VALIDATION_FAILED = "masterAttributesValidationFailed";
        public const string MASTER_ATTRIBUTES_SYNC_FAILED = "masterAttributesSyncFailed";
        public const string MASTER_ATTRIBUTES_LIMIT_EXCEEDED = "masterAttributesLimitExceeded";

        public const string APP_ATTRIBUTES_VALIDATION_FAILED = "appAttributesValidationFailed";
        public const string APP_ATTRIBUTES_SYNC_FAILED = "appAttributesSyncFailed";
        public const string APP_ATTRIBUTES_LIMIT_EXCEEDED = "appAttributesLimitExceeded";

        public const string IDENTITY_DATA_SYNC_FAILED = "identityDataSyncFailed";
        public const string USER_SESSION_EXPIRED = "userSessionExpired";
        public const string USER_IDENTITY_NOT_ENABLED = "userIdentityNotEnabled";
        public const string REFRESH_USER_CREDENTIALS = "refreshUserCredentials";

        // User identity login failure reasons
        public const string LOGIN_IN_PROGRESS = "loginInProgress";
        public const string LOGIN_CONFIG_INVALID = "loginConfigInvalid";
        public const string IDENTITY_TOKEN_INVALID = "identityTokenInvalid";
        public const string IDENTITY_DATA_INVALID = "identitiesDataInvalid";
        public const string LOGIN_CONFIG_SIZE_LIMIT_EXCEEDED = "loginConfigSizeLimitExceeded";
        public const string IDENTITIES_SIZE_LIMIT_EXCEEDED = "identitiesSizeLimitExceeded";
        public const string IDENTITY_FEATURE_NOT_ENABLED = "identityFeatureNotEnabled";
        public const string UID_OR_EMAIL_IS_MANDATORY = "uidOrEmailIsMandatory";
        public const string IAT_IS_MANDATORY = "iatIsMandatory";
        public const string NETWORK_ERROR = "networkError";
        public const string UNKNOWN_ERROR = "unknownError";

        // User identity login error values
        public const string INVALID_VALUE_TYPE = "invalidValueType";
        public const string INVALID_DATA = "invalidData";
        public const string EXCEEDED_KEY_LENGTH_LIMIT = "exceededKeyLengthLimit";
        public const string METADATA_EXCEEDED_KEY_LENGTH_LIMIT = "metadataExceededKeyLengthLimit";
        public const string EXCEEDED_VALUE_LENGTH_LIMIT = "exceededValueLengthLimit";
        public const string METADATA_EXCEEDED_VALUE_LENGTH_LIMIT = "metadataExceededValueLengthLimit";
        public const string EMPTY_DATA = "emptyData";
        public const string METADATA_EMPTY_KEY_OR_VALUE = "metadataEmptyKeyOrValue";
        public const string EXCEEDED_COUNT_LIMIT = "exceededCountLimit";
        public const string METADATA_EXCEEDED_COUNT_LIMIT = "metadataExceededCountLimit";
    }
}
