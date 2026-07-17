public class test {
    public  static int scores(int a){
        return a*100;
    }
    public static void main(String argv[]){
         System.out.println("\033c\033[47;30m\nyou score?\n");
         System.out.println(scores(10));
    }

}
