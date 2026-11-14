using AwesomeAssertions;

using Sentyl.Untis.Models;

namespace Sentyl.Tests.Untis;

public class MasterDataTests
{
    [Fact]
    public void GetTeacherById_KnownId_ReturnsTeacher()
    {
        const int TeacherId = 1;
        var teacher = new Teacher(TeacherId, "John", "Doe", "JODO");
        var data = new MasterData([], teachers: [teacher], [], []);

        var result = data.GetTeacherById(TeacherId);

        result.Should().Be(teacher);
    }

    [Fact]
    public void GetTeacherById_UnknownId_ReturnsNull()
    {
        const int TeacherId = 1;
        const int UnknownTeacherId = 12;

        var teacher = new Teacher(TeacherId, "John", "Doe", "JODO");
        var data = new MasterData([], teachers: [teacher], [], []);

        var result = data.GetTeacherById(UnknownTeacherId);

        result.Should().BeNull();
    }

}
