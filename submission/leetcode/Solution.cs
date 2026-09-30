public class Solution
{
    public int MaxOperations(int[] nums, int k)
    {
        Array.Sort(nums);
        int left = 0;
        int right = nums.Length - 1;
        int operations = 0;

        while (left < right)
        {
            long sum = (long)nums[left] + nums[right];
            if (sum == k)
            {
                operations++;
                left++;
                right--;
            }
            else if (sum < k)
            {
                left++;
            }
            else
            {
                right--;
            }
        }

        return operations;
    }
}
