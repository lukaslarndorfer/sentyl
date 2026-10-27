using AwesomeAssertions;

using Sentyl.Untis;

namespace Sentyl.Tests;


public class MasterDataTests
{
    [Fact]
    public void GetTeacherById_KnownId_ReturnsTeacher()
    {
        const int teacherId = 1;
        var teacher = new Teacher(teacherId, "John", "Doe", "JODO");
        var data = new MasterData([], teachers: [teacher], [], []);

        var result = data.GetTeacherById(teacherId);

        result.Should().Be(teacher);
    }

    [Fact]
    public void GetTeacherById_UnknownId_ReturnsNull()
    {
        const int teacherId = 1;
        const int unknownTeacherId = 12;

        var teacher = new Teacher(teacherId, "John", "Doe", "JODO");
        var data = new MasterData([], teachers: [teacher], [], []);

        var result = data.GetTeacherById(unknownTeacherId);

        result.Should().BeNull();
    }


}
