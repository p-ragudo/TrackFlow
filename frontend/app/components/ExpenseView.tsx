import { View, Text, StyleSheet } from "react-native";
import { Expense } from "../types/Expense";

interface ExpenseViewProps {
    expense: Expense
}

export default function ExpenseView({ expense }: ExpenseViewProps) {
    return (
        <View
            style={styles.section}
        >
            <Text style={styles.groupText}>
                {expense.group}
            </Text>

            <View style={styles.topRow}>
                <Text style={styles.topRowText}>
                    {expense.name}
                </Text>
                <Text style={styles.topRowText}>
                    ₱{expense.amount}
                </Text>
            </View>

            <Text style={styles.categoryText}>
                {expense.category}
            </Text>

            <Text style={styles.tagText}>
                {expense.tag}
            </Text>

            <Text style={[
                expense.description.trim() ? styles.descriptionText: styles.noDescriptionText,
                {marginTop: 12}
            ]}>
                {expense.description.trim() ? expense.description : 'No description'}
            </Text>
        </View>
    )
}

const styles = StyleSheet.create({
    section: {
        paddingHorizontal: 20,
        paddingVertical: 16,
        backgroundColor: 'white',

        borderRadius: 20,
        borderWidth: 0.1,
        borderColor: 'gray'
    },
    topRow: {
        flexDirection: 'row',
        justifyContent: 'space-between',
        marginBottom: 4
    },
    topRowText: {
        fontWeight: 700,
        fontSize: 18
    },
    groupText: {
        fontWeight: 800,
        fontSize: 12,
        color: 'gray'
    },
    categoryText: {
        fontWeight: 500,
        fontSize: 12,
        color: 'gray'
    },
    tagText: {
        fontWeight: 500,
        fontSize: 12,
        color: 'gray'
    },
    descriptionText: {
        fontWeight: 500,
        fontSize: 12,
        color: 'gray'
    },
    noDescriptionText: {
        fontWeight: 300,
        fontSize: 12,
        color: 'gray'
    }
})