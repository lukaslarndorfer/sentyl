namespace Sentyl.Untis;

public sealed class MasterDataLoader(
    UntisClient client,
    ILogger<MasterDataLoader> logger)
{
    public async Task<MasterData> LoadAsync(CancellationToken cancellationToken)
    {
        List<SchoolClass> schoolClasses = await client.GetSchoolClassesAsync(cancellationToken);
        List<Teacher> teachers = await client.GetTeachersAsync(cancellationToken);
        List<Subject> subjects = await client.GetSubjectsAsync(cancellationToken);
        List<Room> rooms = await client.GetRoomsAsync(cancellationToken);

        logger.LogInformation(
            "Fetched master data; school classes: {SchoolClassCount}, teachers: {TeacherCount}, subjects: {SubjectCount}, rooms: {RoomCount}",
            schoolClasses.Count,
            teachers.Count,
            subjects.Count,
            rooms.Count);

        return new MasterData(schoolClasses, teachers, subjects, rooms);
    }

}
