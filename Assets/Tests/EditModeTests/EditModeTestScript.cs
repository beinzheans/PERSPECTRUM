using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Linq;

[TestFixture]
public class EditModeTestScript
{
    private static List<EditorHitbox> CreateHitBoxes(int numberOfHitboxes)
    {
        List<EditorHitbox> results = new();
        for (int i = 0; i < numberOfHitboxes; i++)
        {
            EditorHitbox h = new(new Vector2(0.5f, 0.5f), 100, HitboxType.A, (double)i);
            results.Add(h);
        }

        return results;
    }

    private static IEnumerable<TestCaseData> CreateOnSelectTestCaseData()
    {
        yield return new TestCaseData(0, new int[] { }).Returns(new int[] { }).SetName("Nothing");
        yield return new TestCaseData(1, new int[] { 0 }).Returns(new int[] { 0 }).SetName("Only one, selected");
        yield return new TestCaseData(1, new int[] { }).Returns(new int[] { }).SetName("Only one, not selected");

        yield return new TestCaseData(4, new int[] { 0, 1, 2, 3 }).Returns(new int[] { 0, 1, 2, 3 }).SetName("Select all once");
        yield return new TestCaseData(4, new int[] { } ).Returns(new int[] { }).SetName("Select none");
        yield return new TestCaseData(4, new int[] { 0, 0 }).Returns(new int[] { }).SetName("Repeated select - only 1 repeated");
        yield return new TestCaseData(4, new int[] { 0, 1, 2, 3, 0, 1, 2, 3 }).Returns(new int[] { }).SetName("Repeated select - all repeated");
        yield return new TestCaseData(4, new int[] { 0, 0, 0, 0, 0, 0, 0, 0 }).Returns(new int[] { }).SetName("Repeated select - 8 repeated");
        yield return new TestCaseData(4, new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }).Returns(new int[] { 0 }).SetName("Repeated select - 11 repeated");
        yield return new TestCaseData(4, new int[] { 0, 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3 }).Returns(new int[] { 0, 1, 2, 3 }).SetName("Repeated select - 3 repeats by all");
        yield return new TestCaseData(4, new int[] { 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0, 0, 1, 2, 3, 3, 2, 1, 0 }).Returns(new int[] { }).SetName("Repeated select - 6 repeats by all");

        yield return new TestCaseData(100, new int[] { 0, 51, 32, 93 }).Returns(new int[] { 0, 51, 32, 93 }).SetName("Select few from large list");
        yield return new TestCaseData(100, new int[] { 0, 51, 32, 93, 34, 56, 24, 22, 53, 23, 57, 73, 67, 33, 90, 82, 85, 87, 13, 17 }).Returns(new int[] { 0, 51, 32, 93, 34, 56, 24, 22, 53, 23, 57, 73, 67, 33, 90, 82, 85, 87, 13, 17 }).SetName("Select a lot from large list");
        yield return new TestCaseData(100, new int[] { 0, 51, 51, 93, 34, 56, 56, 34, 51, 51, 34, 77 }).Returns(new int[] { 0, 93, 34, 77 }).SetName("Repeated select from large list");
    }

    [TestCaseSource(nameof(CreateOnSelectTestCaseData))]
    public int[] OnSelect_TestForSelectedStates(int numberOfHitboxes, int[] selectIndexOrder)
    {
        List<EditorHitbox> list = CreateHitBoxes(numberOfHitboxes);
        List<EditorHitbox> selected = new();

        for (int i = 0; i < selectIndexOrder.Length; i++)
        {
            int index = selectIndexOrder[i];
            list[index].OnSelect();
        }

        int[] temp = selected.Select(x =>
        {
            return list.IndexOf(x);
        }).ToArray();

        return temp;
    }

    private static IEnumerable<TestCaseData> CreateOnPlaceDeleteTestCaseData()
    {
        yield return new TestCaseData(0, new int[] { }, new int[] { }).Returns(new int[] { }).SetName("Nothing");
        yield return new TestCaseData(1, new int[] { }, new int[] { }).Returns(new int[] { }).SetName("Only one - nothing");
        yield return new TestCaseData(1, new int[] { 0 }, new int[] { }).Returns(new int[] { 0 }).SetName("Only one - place only");
        yield return new TestCaseData(1, new int[] { }, new int[] { 0 }).Returns(new int[] { }).SetName("Only one - delete only");
        yield return new TestCaseData(1, new int[] { 0 }, new int[] { 0 }).Returns(new int[] { }).SetName("Only one - place & delete");
        yield return new TestCaseData(1, new int[] { 0, 0 }, new int[] { }).Returns(new int[] { 0 }).SetName("Only one - repeated place only");
        yield return new TestCaseData(1, new int[] { }, new int[] { 0, 0 }).Returns(new int[] { }).SetName("Only one - repeated delete only");
        yield return new TestCaseData(1, new int[] { 0 }, new int[] { 0, 0 }).Returns(new int[] { }).SetName("Only one - place & repeated delete");
        yield return new TestCaseData(1, new int[] { 0, 0 }, new int[] { 0, 0 }).Returns(new int[] { }).SetName("Only one - repeated place & repeated delete");

        yield return new TestCaseData(4, new int[] { }, new int[] {}).Returns(new int[] { }).SetName("Small list - do nothing");
        yield return new TestCaseData(4, new int[] { 0, 1, 2, 3 }, new int[] { }).Returns(new int[] { 0, 1, 2, 3 }).SetName("Small list - place all only");
        yield return new TestCaseData(4, new int[] { 0, 1, 2, 3 }, new int[] { 0, 1, 2, 3}).Returns(new int[] { }).SetName("Small list - place then delete all");
        yield return new TestCaseData(4, new int[] { 0, 1, 2, 3 }, new int[] { 1, 3 }).Returns(new int[] { 0, 2 }).SetName("Small list - place all then delete some");
        yield return new TestCaseData(4, new int[] { 0, 1, 2, 3, 3, 3, 3, 3 }, new int[] { 1, 3 }).Returns(new int[] { 0, 2 }).SetName("Small list - repeated place some then delete repeat");
        yield return new TestCaseData(4, new int[] { 0, 1, 2, 3, 3, 3, 3, 3 }, new int[] { 1 }).Returns(new int[] { 0, 2, 3 }).SetName("Small list - repeated place some then delete others");
        yield return new TestCaseData(4, new int[] { 0, 3, }, new int[] { 1, 2 }).Returns(new int[] { 0, 3 }).SetName("Small list -  place some then delete excessively");

        yield return new TestCaseData(100, new int[] { 0, 52, 39, 22, 33, 78, 24 }, new int[] { }).Returns(new int[] { 0, 52, 39, 22, 33, 78, 24 }).SetName("Large list - place some only");
        yield return new TestCaseData(100, new int[] { 0, 52, 39, 22, 33, 78, 24 }, new int[] { 39, 33}).Returns(new int[] { 0, 52, 22, 78, 24 }).SetName("Large list - place some then delete part");
        yield return new TestCaseData(100, new int[] { 0, 52, 39, 22, 33, 78, 24 }, new int[] { 39, 33, 99, 98, 97, 96, 55, 21, 12, 54 }).Returns(new int[] { 0, 52, 22, 78, 24 }).SetName("Large list - place some then delete excessive parts");
    }

    [TestCaseSource(nameof(CreateOnPlaceDeleteTestCaseData))]
    public int[] OnPlaceDelete_TestForPlaceLogic(int numberOfHitboxes, int[] placeIndexOrder, int[] deleteIndexOrder)
    {
        List<EditorHitbox> list = CreateHitBoxes(numberOfHitboxes);
        List<EditorHitbox> placeDeleteables = new();

        for (int i = 0; i < placeIndexOrder.Length; i++)
        {
            int index = placeIndexOrder[i];
            list[index].OnPlace(ref placeDeleteables);
        } 

        for (int i = 0; i < deleteIndexOrder.Length; i++)
        {
            int index = deleteIndexOrder[i];
            list[index].OnDelete(ref placeDeleteables);
        }

        int[] temp = placeDeleteables.Select(x =>
        {
            return list.IndexOf(x);
        }).ToArray();

        return temp;
    }
}